import AppKit
import SpriteKit

/// Something a flake can rest on.
enum SurfaceID: Hashable {
    case window(CGWindowID)
    case dock(Int)
}

final class Flake {
    enum State {
        case falling
        case resting(SurfaceID, offset: CGPoint)
    }

    let node: SKSpriteNode
    /// Position in global CoreGraphics coordinates (y grows downward).
    var pos = CGPoint.zero
    var vel = CGVector.zero
    var radius: CGFloat = 2
    /// 0 = far away (small, faint, slow), 1 = close (big, bright, fast).
    var depth: CGFloat = 1
    var phase: CGFloat = 0
    var wobbleFreq: CGFloat = 1
    var wobbleAmp: CGFloat = 10
    var baseAlpha: CGFloat = 1
    var spin: CGFloat = 0
    var age: TimeInterval = 0
    var landedAt: TimeInterval = 0
    var restUntil: TimeInterval = 0
    var noSettleUntil: TimeInterval = 0
    /// Resting on a window that is covered, or currently off screen.
    var occluded = false
    var state: State = .falling

    init(node: SKSpriteNode) { self.node = node }

    var canSettle: Bool { depth > 0.4 }
}

/// One scene per display. All physics runs in global CoreGraphics coordinates so
/// flakes line up with the window frames reported by `WindowTracker`.
///
/// Settled snow is drawn as a continuous "cap" per surface, built from a height map.
/// A landing flake is visible for a moment, then fades into the cap; it still owns its
/// share of the cap, so when it is knocked loose or expires the cap shrinks with it.
@MainActor
final class SnowScene: SKScene {
    /// The display's rect in global CoreGraphics coordinates.
    let screenRect: CGRect
    /// Height of the primary display, for converting Cocoa → CoreGraphics coordinates.
    let primaryHeight: CGFloat

    private var flakes: [Flake] = []
    private var pool: [SKSpriteNode] = []
    private var spawnAccumulator: Double = 0
    private var lastUpdate: TimeInterval = 0
    private var clock: TimeInterval = 0
    private var restingCount = 0
    private var airborneCount = 0
    private var piles: [SurfaceID: [Int: CGFloat]] = [:]
    private var caps: [SurfaceID: SKShapeNode] = [:]
    private var capsDirty = false
    private var lastCapBuild: TimeInterval = 0
    private var trackerToken: UUID?
    private var prewarmed = false

    private var mouse: CGPoint?
    private var mouseVel = CGVector.zero

    private let maxResting = 3000
    private let pileBucket: CGFloat = 4
    private let maxPileHeight: CGFloat = 14
    private let settleChance: CGFloat = 0.55
    /// macOS windows have rounded top corners; don't let snow hang in the air beside them.
    private let cornerInset: CGFloat = 9
    private let absorbTime: TimeInterval = 1.2
    private let pointerRadius: CGFloat = 80
    private let capZ: CGFloat = 0.45

    // MARK: - Textures

    private static let dotTexture: SKTexture = {
        let side: CGFloat = 32
        let image = NSImage(size: NSSize(width: side, height: side), flipped: false) { rect in
            // Soft white core with a faint grey rim so flakes stay visible on white windows.
            let gradient = NSGradient(
                colors: [
                    NSColor.white,
                    NSColor.white.withAlphaComponent(0.92),
                    NSColor(white: 0.55, alpha: 0.22),
                    NSColor(white: 0.5, alpha: 0),
                ],
                atLocations: [0, 0.45, 0.75, 1],
                colorSpace: .sRGB
            )
            gradient?.draw(in: NSBezierPath(ovalIn: rect), relativeCenterPosition: .zero)
            return true
        }
        return SKTexture(image: image)
    }()

    /// A six-armed crystal used for a few of the closest flakes.
    private static let crystalTexture: SKTexture = {
        let side: CGFloat = 64
        let image = NSImage(size: NSSize(width: side, height: side), flipped: false) { _ in
            let c = CGPoint(x: side / 2, y: side / 2)
            let arm = side * 0.44
            let path = NSBezierPath()
            for k in 0..<6 {
                let a = CGFloat(k) * .pi / 3
                let dir = CGPoint(x: cos(a), y: sin(a))
                path.move(to: c)
                path.line(to: CGPoint(x: c.x + dir.x * arm, y: c.y + dir.y * arm))
                for (at, len) in [(0.45, 0.22), (0.72, 0.14)] as [(CGFloat, CGFloat)] {
                    let base = CGPoint(x: c.x + dir.x * arm * at, y: c.y + dir.y * arm * at)
                    for sign in [-1.0, 1.0] as [CGFloat] {
                        let b = a + sign * .pi / 3.2
                        path.move(to: base)
                        path.line(to: CGPoint(x: base.x + cos(b) * arm * len, y: base.y + sin(b) * arm * len))
                    }
                }
            }
            path.lineCapStyle = .round
            NSColor(white: 0.45, alpha: 0.3).setStroke()
            path.lineWidth = 5
            path.stroke()
            NSColor.white.setStroke()
            path.lineWidth = 2.4
            path.stroke()
            return true
        }
        return SKTexture(image: image)
    }()

    // MARK: - Lifecycle

    init(screenRect: CGRect, primaryHeight: CGFloat) {
        self.screenRect = screenRect
        self.primaryHeight = primaryHeight
        super.init(size: screenRect.size)
        scaleMode = .resizeFill
        anchorPoint = .zero
        backgroundColor = .clear
    }

    required init?(coder aDecoder: NSCoder) { fatalError("init(coder:) is not supported") }

    override func didMove(to view: SKView) {
        if trackerToken == nil {
            trackerToken = WindowTracker.shared.addObserver { [weak self] events in
                self?.handle(events)
            }
        }
        if !prewarmed {
            prewarmed = true
            prewarm()
        }
    }

    override func willMove(from view: SKView) {
        if let token = trackerToken {
            WindowTracker.shared.removeObserver(token)
            trackerToken = nil
        }
    }

    // MARK: - Public controls

    /// Knocks every settled flake loose, as if the screen were given a shake.
    func shakeOffSettledSnow() {
        for flake in flakes {
            guard case .resting = flake.state else { continue }
            if flake.occluded {
                flake.restUntil = -1 // removed next frame
            } else {
                release(flake, velocity: CGVector(dx: .random(in: -70...70), dy: -.random(in: 60...200)))
            }
        }
    }

    // MARK: - Frame loop

    override func update(_ currentTime: TimeInterval) {
        let rawDt = lastUpdate == 0 ? 1.0 / 60.0 : currentTime - lastUpdate
        lastUpdate = currentTime
        // Clamp so a long pause doesn't teleport everything.
        let dt = min(max(rawDt, 0), 1.0 / 20.0)
        clock += dt

        let settings = Settings.shared
        let tracker = WindowTracker.shared
        let pointerActive = updatePointer(dt: dt, enabled: settings.pointer)

        spawn(dt: dt, settings: settings, density: PowerMonitor.shared.densityFactor)

        let wind = currentWind(settings.wind)
        let settleEnabled = settings.settle
        var airborne = 0

        var i = 0
        while i < flakes.count {
            let flake = flakes[i]
            flake.age += dt
            var alive = true

            switch flake.state {
            case .falling:
                if pointerActive { stir(flake, dt: dt) }
                alive = stepFalling(flake, dt: dt, wind: wind, settleEnabled: settleEnabled)
                if case .falling = flake.state { airborne += 1 }

            case let .resting(surface, offset):
                guard let base = origin(of: surface, tracker: tracker) else {
                    // Window is off screen (minimized, other Space...): keep its snow for later.
                    flake.occluded = true
                    alive = flake.age < flake.restUntil
                    break
                }
                if flake.age > flake.restUntil {
                    alive = false
                } else if !settleEnabled {
                    release(flake, velocity: .zero)
                } else {
                    flake.pos = CGPoint(x: base.x + offset.x, y: base.y + offset.y)
                    if pointerActive && !flake.occluded { knock(flake) }
                }
            }

            if alive {
                render(flake, dt: dt)
                i += 1
            } else {
                remove(at: i)
            }
        }
        airborneCount = airborne

        updateCaps(tracker: tracker)
    }

    private func render(_ flake: Flake, dt: TimeInterval) {
        let node = flake.node
        if case .resting = flake.state {
            let t = (flake.age - flake.landedAt) / absorbTime
            if t >= 1 {
                // Fully merged into the cap: take the sprite off the render tree.
                if node.parent != nil { node.removeFromParent() }
                return
            }
            node.alpha = flake.baseAlpha * CGFloat(1 - t)
            node.isHidden = flake.occluded
        }
        node.position = scenePoint(flake.pos)
        if flake.spin != 0 { node.zRotation += flake.spin * CGFloat(dt) }
    }

    // MARK: - Simulation

    private func currentWind(_ wind: Wind) -> CGFloat {
        let base = wind.strength
        guard base > 0 else { return 0 }
        // Slow swells, plus sharper gusts when it's gusty.
        var w = base * (0.65 + 0.35 * sin(CGFloat(clock) * 0.21))
        if wind == .gusty {
            w += base * 0.8 * max(0, sin(CGFloat(clock) * 0.57) * sin(CGFloat(clock) * 0.13))
        }
        return w
    }

    private func spawn(dt: TimeInterval, settings: Settings, density: Double) {
        let intensity = settings.intensity
        let cap = Int(Double(intensity.maxFalling) * density)
        spawnAccumulator += intensity.spawnRate * density * Double(screenRect.width) / 1000 * dt
        var airborne = airborneCount
        while spawnAccumulator >= 1 {
            spawnAccumulator -= 1
            guard airborne < cap else { continue }
            spawnFlake(windStrength: settings.wind.strength, y: nil)
            airborne += 1
        }
        airborneCount = airborne
    }

    /// Fills the sky on launch so the screen isn't empty while the first flakes fall.
    private func prewarm() {
        let settings = Settings.shared
        let density = PowerMonitor.shared.densityFactor
        let perSecond = settings.intensity.spawnRate * density * Double(screenRect.width) / 1000
        let averageFallTime = Double(screenRect.height) / 65
        let count = min(Int(perSecond * averageFallTime), Int(Double(settings.intensity.maxFalling) * density))
        for _ in 0..<max(count, 0) {
            spawnFlake(windStrength: settings.wind.strength,
                       y: .random(in: screenRect.minY...screenRect.maxY))
        }
        airborneCount = flakes.count
    }

    private func spawnFlake(windStrength: CGFloat, y: CGFloat?) {
        let node = pool.popLast() ?? SKSpriteNode(texture: Self.dotTexture)
        let flake = Flake(node: node)
        // Bias towards distant flakes for a sense of depth.
        flake.depth = pow(CGFloat.random(in: 0...1), 0.8)
        flake.baseAlpha = 0.4 + 0.6 * flake.depth
        flake.wobbleAmp = 6 + 16 * flake.depth
        flake.wobbleFreq = .random(in: 0.6...1.6)
        flake.phase = .random(in: 0...(2 * .pi))

        let isCrystal = flake.depth > 0.8 && CGFloat.random(in: 0...1) < 0.18
        let diameter: CGFloat
        if isCrystal {
            flake.radius = .random(in: 4...6.5)
            flake.spin = .random(in: 0.3...1.1) * (Bool.random() ? 1 : -1)
            node.texture = Self.crystalTexture
            node.zRotation = .random(in: 0...(2 * .pi))
            diameter = flake.radius * 2 * 1.3
        } else {
            flake.radius = 0.9 + flake.depth * 3.1
            flake.spin = 0
            node.texture = Self.dotTexture
            node.zRotation = 0
            diameter = flake.radius * 2 * 1.5
        }

        // Spawn a little upwind so the left edge isn't bare in a breeze.
        let upwind = windStrength * 3
        let x = CGFloat.random(in: (screenRect.minX - upwind)...(screenRect.maxX + 20))
        flake.pos = CGPoint(x: x, y: y ?? screenRect.minY - .random(in: 4...30))
        flake.vel = CGVector(dx: 0, dy: terminalSpeed(flake))

        node.size = CGSize(width: diameter, height: diameter)
        node.alpha = flake.baseAlpha
        node.isHidden = false
        node.zPosition = flake.depth
        node.position = scenePoint(flake.pos)
        addChild(node)
        flakes.append(flake)
    }

    private func terminalSpeed(_ flake: Flake) -> CGFloat {
        28 + 80 * flake.depth
    }

    /// Returns false when the flake has left the screen.
    private func stepFalling(_ flake: Flake, dt: TimeInterval, wind: CGFloat, settleEnabled: Bool) -> Bool {
        let t = CGFloat(clock)
        let targetX = wind * (0.5 + flake.depth) + sin(t * flake.wobbleFreq + flake.phase) * flake.wobbleAmp
        let targetY = terminalSpeed(flake)
        // Relax toward drift velocity: flung flakes lose momentum like light snow in air.
        let k = min(1, 1.6 * CGFloat(dt))
        flake.vel.dx += (targetX - flake.vel.dx) * k
        flake.vel.dy += (targetY - flake.vel.dy) * k

        let old = flake.pos
        let new = CGPoint(x: old.x + flake.vel.dx * CGFloat(dt), y: old.y + flake.vel.dy * CGFloat(dt))
        flake.pos = new

        if settleEnabled, flake.canSettle, flake.vel.dy > 0, flake.age >= flake.noSettleUntil,
           restingCount < maxResting {
            if tryLand(flake, from: old, to: new) { return true }
        }

        if new.y > screenRect.maxY + 10 { return false }
        if new.x < screenRect.minX - 240 || new.x > screenRect.maxX + 240 { return false }
        return true
    }

    private func tryLand(_ flake: Flake, from old: CGPoint, to new: CGPoint) -> Bool {
        let tracker = WindowTracker.shared

        for (index, window) in tracker.windows.enumerated() {
            let frame = window.frame
            guard new.x > frame.minX + cornerInset, new.x < frame.maxX - cornerInset else { continue }
            let surface = SurfaceID.window(window.id)
            let top = frame.minY - pileHeight(surface, x: new.x - frame.minX)
            guard old.y <= top, new.y >= top else { continue }
            // Only land on the exposed part of the top edge.
            guard tracker.isPointVisible(CGPoint(x: new.x, y: frame.minY + 2), inFrontOf: index),
                  tracker.isPointVisible(CGPoint(x: new.x, y: top - 2), inFrontOf: index)
            else { continue }
            guard CGFloat.random(in: 0...1) < settleChance else { return false }
            settle(flake, on: surface, base: frame.origin, top: top)
            return true
        }

        for (index, dock) in tracker.dockRects.enumerated() where isPlausibleDock(dock) {
            guard new.x > dock.minX + cornerInset, new.x < dock.maxX - cornerInset else { continue }
            let surface = SurfaceID.dock(index)
            let top = dock.minY - pileHeight(surface, x: new.x - dock.minX)
            guard old.y <= top, new.y >= top else { continue }
            guard tracker.isPointUncovered(CGPoint(x: new.x, y: top - 2)) else { continue }
            guard CGFloat.random(in: 0...1) < settleChance else { return false }
            settle(flake, on: surface, base: dock.origin, top: top)
            return true
        }
        return false
    }

    /// The Dock's window is only used if it looks like the Dock itself: a strip on this
    /// display's bottom edge that is narrower than the screen. (A full-width window would
    /// make snow hover over the desktop beside the Dock.)
    private func isPlausibleDock(_ rect: CGRect) -> Bool {
        rect.intersects(screenRect)
            && rect.height < screenRect.height * 0.35
            && rect.width < screenRect.width * 0.97
            && rect.maxY >= screenRect.maxY - 4
    }

    private func settle(_ flake: Flake, on surface: SurfaceID, base: CGPoint, top: CGFloat) {
        let restY = top - flake.radius * 0.45
        flake.pos.y = restY
        flake.vel = .zero
        let offset = CGPoint(x: flake.pos.x - base.x, y: restY - base.y)
        flake.state = .resting(surface, offset: offset)
        flake.landedAt = flake.age
        flake.restUntil = flake.age + .random(in: 45...150)
        flake.occluded = false
        restingCount += 1
        adjustPile(surface, x: offset.x, radius: flake.radius, sign: 1)
    }

    /// Knocks a resting flake back into the air.
    private func release(_ flake: Flake, velocity: CGVector) {
        guard case let .resting(surface, offset) = flake.state else { return }
        adjustPile(surface, x: offset.x, radius: flake.radius, sign: -1)
        restingCount -= 1
        flake.state = .falling
        flake.vel = velocity
        flake.noSettleUntil = flake.age + 0.35
        flake.occluded = false
        flake.node.alpha = flake.baseAlpha
        flake.node.isHidden = false
        flake.node.position = scenePoint(flake.pos)
        if flake.node.parent == nil { addChild(flake.node) }
    }

    private func remove(at index: Int) {
        let flake = flakes[index]
        if case let .resting(surface, offset) = flake.state {
            adjustPile(surface, x: offset.x, radius: flake.radius, sign: -1)
            restingCount -= 1
        }
        flake.node.removeFromParent()
        pool.append(flake.node)
        flakes.swapAt(index, flakes.count - 1)
        flakes.removeLast()
    }

    // MARK: - Pointer

    /// Tracks the pointer (no permission needed) and returns whether it should
    /// interact with snow on this display this frame.
    private func updatePointer(dt: TimeInterval, enabled: Bool) -> Bool {
        guard enabled else { mouse = nil; return false }
        let m = NSEvent.mouseLocation
        let p = CGPoint(x: m.x, y: primaryHeight - m.y)
        if let last = mouse, dt > 0 {
            let v = CGVector(dx: (p.x - last.x) / CGFloat(dt), dy: (p.y - last.y) / CGFloat(dt))
            mouseVel.dx += (v.dx - mouseVel.dx) * 0.5
            mouseVel.dy += (v.dy - mouseVel.dy) * 0.5
        }
        mouse = p
        let speed = hypot(mouseVel.dx, mouseVel.dy)
        return speed > 40 && screenRect.insetBy(dx: -pointerRadius, dy: -pointerRadius).contains(p)
    }

    /// Falling flakes near a moving pointer get swept along in its wake.
    private func stir(_ flake: Flake, dt: TimeInterval) {
        guard let m = mouse else { return }
        let dx = flake.pos.x - m.x, dy = flake.pos.y - m.y
        let d2 = dx * dx + dy * dy
        guard d2 < pointerRadius * pointerRadius else { return }
        let d = max(sqrt(d2), 1)
        let w = 1 - d / pointerRadius
        let k = min(1, w * 10 * CGFloat(dt))
        flake.vel.dx += (mouseVel.dx * 0.6 - flake.vel.dx) * k + dx / d * w * 220 * CGFloat(dt)
        flake.vel.dy += (mouseVel.dy * 0.6 - flake.vel.dy) * k + dy / d * w * 220 * CGFloat(dt)
    }

    /// A fast pointer swipe through settled snow kicks it up.
    private func knock(_ flake: Flake) {
        guard let m = mouse else { return }
        let speed = hypot(mouseVel.dx, mouseVel.dy)
        guard speed > 250,
              abs(flake.pos.x - m.x) < 22, abs(flake.pos.y - m.y) < 22 else { return }
        release(flake, velocity: CGVector(
            dx: mouseVel.dx * 0.45 + .random(in: -40...40),
            dy: min(mouseVel.dy * 0.45, 0) - .random(in: 40...140)
        ))
    }

    // MARK: - Piles & caps

    private func pileHeight(_ surface: SurfaceID, x: CGFloat) -> CGFloat {
        piles[surface]?[Int((x / pileBucket).rounded(.down))] ?? 0
    }

    private func adjustPile(_ surface: SurfaceID, x: CGFloat, radius: CGFloat, sign: CGFloat) {
        let lo = Int(((x - radius) / pileBucket).rounded(.down))
        let hi = Int(((x + radius) / pileBucket).rounded(.down))
        var buckets = piles[surface] ?? [:]
        for b in lo...max(lo, hi) {
            let h = (buckets[b] ?? 0) + sign * radius * 0.5
            buckets[b] = min(maxPileHeight, max(0, h))
        }
        piles[surface] = buckets
        capsDirty = true
    }

    private func updateCaps(tracker: WindowTracker) {
        // Rebuilding paths is the expensive part; ~20 Hz is plenty for growth.
        if capsDirty && clock - lastCapBuild > 0.05 {
            rebuildCaps(tracker: tracker)
        }
        for (surface, node) in caps {
            if let o = origin(of: surface, tracker: tracker) {
                node.position = scenePoint(o)
                node.isHidden = false
            } else {
                node.isHidden = true
            }
        }
    }

    private func rebuildCaps(tracker: WindowTracker) {
        capsDirty = false
        lastCapBuild = clock
        var live = Set<SurfaceID>()

        for (surface, buckets) in piles {
            guard let lo = buckets.keys.min(), let hi = buckets.keys.max() else { continue }
            guard let frame = frame(of: surface, tracker: tracker) else {
                // Off screen for now; keep the pile, hide its cap.
                if caps[surface] != nil { live.insert(surface) }
                continue
            }
            let windowIndex: Int?
            if case let .window(id) = surface { windowIndex = tracker.indexByID[id] } else { windowIndex = nil }

            func height(_ b: Int) -> CGFloat {
                let left = buckets[b - 1] ?? 0, mid = buckets[b] ?? 0, right = buckets[b + 1] ?? 0
                return (left + 2 * mid + right) / 4
            }
            func visible(_ x: CGFloat, _ h: CGFloat) -> Bool {
                let edge = CGPoint(x: frame.minX + x, y: frame.minY + 1)
                let crest = CGPoint(x: frame.minX + x, y: frame.minY - h)
                if let index = windowIndex {
                    return tracker.isPointVisible(edge, inFrontOf: index)
                        && tracker.isPointVisible(crest, inFrontOf: index)
                }
                return tracker.isPointUncovered(crest)
            }

            // Trace the snow line left to right, splitting it wherever another window
            // covers this one. Paths are in scene orientation: y up, from the surface's top edge.
            let path = CGMutablePath()
            var open = false
            var lastX: CGFloat = 0
            for b in (lo - 1)...(hi + 1) {
                let x = (CGFloat(b) + 0.5) * pileBucket
                let h = height(b)
                if h > 0.35 && visible(x, h) {
                    if !open {
                        path.move(to: CGPoint(x: x - pileBucket * 0.5, y: 0))
                        open = true
                    }
                    path.addLine(to: CGPoint(x: x, y: h))
                    lastX = x
                } else if open {
                    path.addLine(to: CGPoint(x: lastX + pileBucket * 0.5, y: 0))
                    open = false
                }
            }
            if open { path.addLine(to: CGPoint(x: lastX + pileBucket * 0.5, y: 0)) }

            if path.isEmpty {
                if buckets.values.allSatisfy({ $0 <= 0.01 }) { piles[surface] = nil }
                continue
            }
            live.insert(surface)
            let node = caps[surface] ?? makeCap(for: surface)
            node.path = path
        }

        for (surface, node) in caps where !live.contains(surface) {
            node.removeFromParent()
            caps[surface] = nil
        }
    }

    private func makeCap(for surface: SurfaceID) -> SKShapeNode {
        let node = SKShapeNode()
        node.fillColor = NSColor(white: 1, alpha: 0.97)
        // A faint outline keeps the snow visible on light window chrome.
        node.strokeColor = NSColor(white: 0.55, alpha: 0.35)
        node.lineWidth = 0.8
        node.isAntialiased = true
        node.zPosition = capZ
        addChild(node)
        caps[surface] = node
        return node
    }

    // MARK: - Window events

    private func frame(of surface: SurfaceID, tracker: WindowTracker) -> CGRect? {
        switch surface {
        case let .window(id):
            return tracker.framesByID[id]
        case let .dock(index):
            guard index < tracker.dockRects.count else { return nil }
            return tracker.dockRects[index]
        }
    }

    private func origin(of surface: SurfaceID, tracker: WindowTracker) -> CGPoint? {
        frame(of: surface, tracker: tracker)?.origin
    }

    private func handle(_ events: [SurfaceEvent]) {
        if !events.isEmpty {
            var moved: [CGWindowID: CGVector] = [:]
            var relocated = Set<CGWindowID>()
            for event in events {
                switch event {
                case let .moved(id, velocity): moved[id] = velocity
                case let .relocated(id): relocated.insert(id)
                case .vanished: break // snow stays, hidden, until the window returns
                }
            }

            if !moved.isEmpty || !relocated.isEmpty {
                for flake in flakes {
                    guard case let .resting(.window(id), _) = flake.state else { continue }
                    if relocated.contains(id) {
                        flake.restUntil = -1 // removed next frame
                    } else if let v = moved[id] {
                        if flake.occluded {
                            flake.restUntil = -1
                        } else {
                            // The window was yanked out from under the snow: fling it.
                            release(flake, velocity: CGVector(
                                dx: clamp(v.dx * 0.55, 1400) + .random(in: -40...40),
                                dy: min(clamp(v.dy * 0.55, 1400), 0) - .random(in: 15...90)
                            ))
                        }
                    }
                }
                for id in moved.keys { piles[.window(id)] = nil }
                for id in relocated { piles[.window(id)] = nil }
            }
        }

        // Snow on a window that is now covered is hidden (not lost) until uncovered.
        let tracker = WindowTracker.shared
        for flake in flakes {
            guard case let .resting(surface, _) = flake.state else { continue }
            switch surface {
            case let .window(id):
                if let index = tracker.indexByID[id] {
                    flake.occluded = !tracker.isPointVisible(flake.pos, inFrontOf: index)
                } else {
                    flake.occluded = true
                }
            case .dock:
                flake.occluded = !tracker.isPointUncovered(flake.pos)
            }
        }
        capsDirty = true
        rebuildCaps(tracker: tracker)
    }

    private func clamp(_ value: CGFloat, _ limit: CGFloat) -> CGFloat {
        max(-limit, min(limit, value))
    }

    // MARK: - Coordinates

    private func scenePoint(_ global: CGPoint) -> CGPoint {
        CGPoint(x: global.x - screenRect.minX, y: screenRect.maxY - global.y)
    }
}
