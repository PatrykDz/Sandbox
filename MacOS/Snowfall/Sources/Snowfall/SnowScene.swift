import AppKit
import SpriteKit

/// Something snow can rest on.
enum SurfaceID: Hashable {
    case window(CGWindowID)
    case dock(Int)
}

/// A flake drifting down from the sky.
final class Flake {
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
    var spin: CGFloat = 0

    init(node: SKSpriteNode) { self.node = node }

    var canSettle: Bool { depth > 0.4 }
}

/// A clump of snow that broke off a drift and is falling under gravity
/// (or, when `life > 0`, a short-lived puff of powder).
final class Chunk {
    let node: SKSpriteNode
    var pos = CGPoint.zero
    var vel = CGVector.zero
    var radius: CGFloat = 3
    var age: TimeInterval = 0
    /// 0 for a solid clump; otherwise the powder fades out over this many seconds.
    var life: TimeInterval = 0
    var alpha: CGFloat = 1
    var landAfter: TimeInterval = 0

    init(node: SKSpriteNode) { self.node = node }
}

/// The drift of snow lying on one window top (or the Dock), stored as a height map
/// in the surface's own coordinates so it travels with the window.
///
/// The drift is a physical body: it has its own velocity, is dragged along by
/// friction, and is only held down by gravity.
final class SnowSurface {
    static let dx: CGFloat = 4

    let id: SurfaceID
    var frame: CGRect
    /// Height of snow per column. Column `i` is centred at `(i + 0.5) * dx + shift`.
    var heights: [CGFloat]
    /// How far the drift has slid sideways relative to the window, within ±dx/2.
    var shift: CGFloat = 0
    var onScreen = true
    var lastSeen: TimeInterval = 0
    /// Velocity of the window and of the snow lying on it, in points per second.
    var winVel = CGVector.zero
    var snowVel = CGVector.zero
    /// Distance the drift has lifted off the surface (window dropped away beneath it).
    var gap: CGFloat = 0
    var hasSnow = false
    var dirty = true
    var lastBuild: TimeInterval = 0
    /// Snow that has crumbled over each edge but isn't yet enough for a clump.
    var spillLeft: CGFloat = 0
    var spillRight: CGFloat = 0
    var node: SKShapeNode?

    init(id: SurfaceID, frame: CGRect) {
        self.id = id
        self.frame = frame
        heights = Array(repeating: 0, count: Self.columns(for: frame.width))
    }

    static func columns(for width: CGFloat) -> Int {
        max(1, Int((width / dx).rounded(.up)))
    }

    func x(ofColumn i: Int) -> CGFloat { (CGFloat(i) + 0.5) * Self.dx + shift }
    func column(at localX: CGFloat) -> Int { Int(((localX - shift) / Self.dx).rounded(.down)) }

    func height(at localX: CGFloat) -> CGFloat {
        let i = column(at: localX)
        return i >= 0 && i < heights.count ? heights[i] : 0
    }
}

/// One scene per display. All physics runs in global CoreGraphics coordinates so
/// snow lines up with the window frames reported by `WindowTracker`.
@MainActor
final class SnowScene: SKScene {
    /// The display's rect in global CoreGraphics coordinates.
    let screenRect: CGRect
    /// Height of the primary display, for converting Cocoa → CoreGraphics coordinates.
    let primaryHeight: CGFloat

    private var flakes: [Flake] = []
    private var chunks: [Chunk] = []
    private var surfaces: [SurfaceID: SnowSurface] = [:]
    private var pool: [SKSpriteNode] = []
    private var spawnAccumulator: Double = 0
    private var lastUpdate: TimeInterval = 0
    private var clock: TimeInterval = 0
    private var lastMelt: TimeInterval = 0
    private var trackerToken: UUID?
    private var prewarmed = false

    private var mouse: CGPoint?
    private var mouseVel = CGVector.zero

    // Physics, in points and seconds.
    private let gravity: CGFloat = 1700
    /// Greatest acceleration friction can give the drift before it starts to slide.
    private let grip: CGFloat = 2200
    /// A drift that has lifted this far off its window breaks up.
    private let detachGap: CGFloat = 14
    private let maxHeight: CGFloat = 22
    /// Steepest height difference between neighbouring columns before snow avalanches.
    private let maxStep: CGFloat = 4.5
    private let edgeCliff: CGFloat = 7
    /// Drifts slowly melt, so depth levels off instead of growing forever.
    private let meltTime: Double = 150
    private let settleChance: CGFloat = 0.55
    /// macOS windows have rounded top corners; snow can't sit beside them.
    private let cornerInset: CGFloat = 9
    private let pointerRadius: CGFloat = 80
    private let maxChunks = 2200
    private let capZ: CGFloat = 0.45
    private let chunkZ: CGFloat = 0.6

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

    /// Throws every drift into the air, as if the screen were given a shake.
    func shakeOffSettledSnow() {
        for surface in surfaces.values {
            detach(surface, spread: 90, up: 80...320, landDelay: 30)
        }
    }

    // MARK: - Frame loop

    override func update(_ currentTime: TimeInterval) {
        let rawDt = lastUpdate == 0 ? 1.0 / 60.0 : currentTime - lastUpdate
        lastUpdate = currentTime
        // Clamp so a long pause doesn't teleport everything.
        let dt = min(max(rawDt, 0), 1.0 / 20.0)
        clock += dt
        let fdt = CGFloat(dt)

        let settings = Settings.shared
        let tracker = WindowTracker.shared
        let pointerActive = updatePointer(dt: dt, enabled: settings.pointer)
        let settleEnabled = settings.settle
        let wind = currentWind(settings.wind)

        spawn(dt: dt, settings: settings, density: PowerMonitor.shared.densityFactor)

        // Falling flakes.
        var i = 0
        while i < flakes.count {
            let flake = flakes[i]
            if pointerActive { stir(flake, dt: fdt) }
            if stepFlake(flake, dt: fdt, wind: wind, settleEnabled: settleEnabled) {
                flake.node.position = scenePoint(flake.pos)
                if flake.spin != 0 { flake.node.zRotation += flake.spin * fdt }
                i += 1
            } else {
                recycle(flake.node)
                flakes.swapAt(i, flakes.count - 1)
                flakes.removeLast()
            }
        }

        // Drifts.
        for surface in surfaces.values where surface.hasSnow {
            if !settleEnabled {
                detach(surface, spread: 25, up: 0...30, landDelay: 30)
            } else if surface.onScreen {
                stepSurface(surface, dt: fdt)
            }
        }
        if pointerActive && settleEnabled { carveWithPointer() }

        // Falling clumps and powder.
        i = 0
        while i < chunks.count {
            let chunk = chunks[i]
            if stepChunk(chunk, dt: fdt, wind: wind, settleEnabled: settleEnabled) {
                chunk.node.position = scenePoint(chunk.pos)
                i += 1
            } else {
                recycle(chunk.node)
                chunks.swapAt(i, chunks.count - 1)
                chunks.removeLast()
            }
        }

        if clock - lastMelt > 0.5 {
            melt(elapsed: clock - lastMelt)
            lastMelt = clock
        }

        for surface in surfaces.values {
            let moving = surface.gap > 0 || abs(surface.snowVel.dx - surface.winVel.dx) > 1
            if surface.dirty && (moving || clock - surface.lastBuild > 0.1) {
                relax(surface)
                rebuildCap(surface, tracker: tracker)
            }
            if let node = surface.node {
                var p = scenePoint(surface.frame.origin)
                p.y += surface.gap
                node.position = p
                node.isHidden = !surface.onScreen
            }
        }
    }

    // MARK: - Falling flakes

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
        while spawnAccumulator >= 1 {
            spawnAccumulator -= 1
            guard flakes.count < cap else { continue }
            spawnFlake(windStrength: settings.wind.strength, y: nil)
        }
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
    }

    private func dequeueNode() -> SKSpriteNode {
        let node = pool.popLast() ?? SKSpriteNode(texture: Self.dotTexture)
        node.texture = Self.dotTexture
        node.zRotation = 0
        node.isHidden = false
        return node
    }

    private func recycle(_ node: SKSpriteNode) {
        node.removeFromParent()
        pool.append(node)
    }

    private func spawnFlake(windStrength: CGFloat, y: CGFloat?) {
        let node = dequeueNode()
        let flake = Flake(node: node)
        // Bias towards distant flakes for a sense of depth.
        flake.depth = pow(CGFloat.random(in: 0...1), 0.8)
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
            diameter = flake.radius * 2 * 1.5
        }

        // Spawn a little upwind so the left edge isn't bare in a breeze.
        let upwind = windStrength * 3
        let x = CGFloat.random(in: (screenRect.minX - upwind)...(screenRect.maxX + 20))
        flake.pos = CGPoint(x: x, y: y ?? screenRect.minY - .random(in: 4...30))
        flake.vel = CGVector(dx: 0, dy: terminalSpeed(flake))

        node.size = CGSize(width: diameter, height: diameter)
        node.alpha = 0.4 + 0.6 * flake.depth
        node.zPosition = flake.depth
        node.position = scenePoint(flake.pos)
        addChild(node)
        flakes.append(flake)
    }

    private func terminalSpeed(_ flake: Flake) -> CGFloat {
        28 + 80 * flake.depth
    }

    /// Returns false when the flake is gone (left the screen, or joined a drift).
    private func stepFlake(_ flake: Flake, dt: CGFloat, wind: CGFloat, settleEnabled: Bool) -> Bool {
        let t = CGFloat(clock)
        let targetX = wind * (0.5 + flake.depth) + sin(t * flake.wobbleFreq + flake.phase) * flake.wobbleAmp
        let targetY = terminalSpeed(flake)
        // Flakes are so light that they simply relax toward their drift velocity.
        let k = min(1, 1.6 * dt)
        flake.vel.dx += (targetX - flake.vel.dx) * k
        flake.vel.dy += (targetY - flake.vel.dy) * k

        let old = flake.pos
        let new = CGPoint(x: old.x + flake.vel.dx * dt, y: old.y + flake.vel.dy * dt)
        flake.pos = new

        if settleEnabled, flake.canSettle, flake.vel.dy > 0,
           let (surface, localX) = landing(from: old, to: new, chance: settleChance) {
            deposit(surface, localX: localX, area: flake.radius * flake.radius * 0.9)
            return false
        }

        if new.y > screenRect.maxY + 10 { return false }
        if new.x < screenRect.minX - 240 || new.x > screenRect.maxX + 240 { return false }
        return true
    }

    // MARK: - Landing

    /// Finds the surface whose snow line the segment `old → new` crosses, if any.
    private func landing(from old: CGPoint, to new: CGPoint, chance: CGFloat) -> (SnowSurface, CGFloat)? {
        let tracker = WindowTracker.shared

        for (index, window) in tracker.windows.enumerated() {
            let frame = window.frame
            guard new.x > frame.minX + cornerInset, new.x < frame.maxX - cornerInset else { continue }
            let id = SurfaceID.window(window.id)
            let existing = surfaces[id]
            let localX = new.x - frame.minX
            let top = frame.minY - (existing.map { $0.gap + $0.height(at: localX) } ?? 0)
            guard old.y <= top, new.y >= top else { continue }
            // Only land on the exposed part of the top edge.
            guard tracker.isPointVisible(CGPoint(x: new.x, y: frame.minY + 2), inFrontOf: index),
                  tracker.isPointVisible(CGPoint(x: new.x, y: top - 2), inFrontOf: index)
            else { continue }
            guard chance >= 1 || CGFloat.random(in: 0...1) < chance else { return nil }
            return (existing ?? makeSurface(id, frame: frame), localX)
        }

        for (index, dock) in tracker.dockRects.enumerated() where isPlausibleDock(dock) {
            guard new.x > dock.minX + cornerInset, new.x < dock.maxX - cornerInset else { continue }
            let id = SurfaceID.dock(index)
            let existing = surfaces[id]
            let localX = new.x - dock.minX
            let top = dock.minY - (existing?.height(at: localX) ?? 0)
            guard old.y <= top, new.y >= top else { continue }
            guard tracker.isPointUncovered(CGPoint(x: new.x, y: top - 2)) else { continue }
            guard chance >= 1 || CGFloat.random(in: 0...1) < chance else { return nil }
            return (existing ?? makeSurface(id, frame: dock), localX)
        }
        return nil
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

    private func makeSurface(_ id: SurfaceID, frame: CGRect) -> SnowSurface {
        let surface = SnowSurface(id: id, frame: frame)
        surface.lastSeen = clock
        surfaces[id] = surface
        return surface
    }

    /// Adds `area` (square points) of snow around `localX`, spread over a few columns.
    private func deposit(_ s: SnowSurface, localX: CGFloat, area: CGFloat) {
        let c = s.column(at: localX)
        let lo = cornerInset, hi = s.frame.width - cornerInset
        let weights: [CGFloat] = [1, 2, 3, 2, 1]
        for (k, w) in weights.enumerated() {
            let i = c + k - 2
            guard i >= 0, i < s.heights.count else { continue }
            let x = s.x(ofColumn: i)
            guard x >= lo, x <= hi else { continue }
            s.heights[i] = min(maxHeight, s.heights[i] + area * w / 9 / SnowSurface.dx)
        }
        s.hasSnow = true
        s.dirty = true
    }

    // MARK: - Drift physics

    private func stepSurface(_ s: SnowSurface, dt: CGFloat) {
        // Sideways: friction drags the drift toward the window's velocity, but can only
        // accelerate it so fast. Jerk the window and the snow is left behind, sliding.
        let diff = s.winVel.dx - s.snowVel.dx
        let maxChange = grip * dt
        s.snowVel.dx += max(-maxChange, min(maxChange, diff))
        let slide = s.snowVel.dx - s.winVel.dx
        if abs(slide) > 1 {
            s.shift += slide * dt
            normalizeShift(s)
            shedEdges(s)
            s.dirty = true
        }

        // Vertically the window can push the drift up but not pull it down: if the
        // window drops away faster than gravity, or stops after rising, the snow lifts off.
        s.snowVel.dy += gravity * dt
        s.gap += (s.winVel.dy - s.snowVel.dy) * dt
        if s.gap <= 0 {
            s.gap = 0
            s.snowVel.dy = s.winVel.dy
        } else if s.gap > detachGap {
            detach(s, spread: 22, up: -15...25, landDelay: 0)
        }
    }

    /// Keeps `shift` within half a column by moving whole columns along the array.
    private func normalizeShift(_ s: SnowSurface) {
        let dx = SnowSurface.dx
        while s.shift > dx / 2 {
            s.shift -= dx
            let fallen = s.heights.removeLast()
            s.heights.insert(0, at: 0)
            if fallen > 0 { spill(from: s, column: s.heights.count, height: fallen) }
        }
        while s.shift < -dx / 2 {
            s.shift += dx
            let fallen = s.heights.removeFirst()
            s.heights.append(0)
            if fallen > 0 { spill(from: s, column: -1, height: fallen) }
        }
    }

    /// Snow pushed past either end of the top edge tumbles off.
    private func shedEdges(_ s: SnowSurface) {
        let lo = cornerInset, hi = s.frame.width - cornerInset
        let n = s.heights.count
        let reach = Int(cornerInset / SnowSurface.dx) + 3
        for i in 0..<min(reach, n) where s.heights[i] > 0 && s.x(ofColumn: i) < lo {
            spill(from: s, column: i, height: s.heights[i])
            s.heights[i] = 0
        }
        for i in max(0, n - reach)..<n where s.heights[i] > 0 && s.x(ofColumn: i) > hi {
            spill(from: s, column: i, height: s.heights[i])
            s.heights[i] = 0
        }
    }

    /// Turns one column of snow going over an edge into a falling clump.
    private func spill(from s: SnowSurface, column: Int, height: CGFloat) {
        guard s.onScreen else { return }
        let x = s.frame.minX + s.x(ofColumn: column)
        let point = CGPoint(x: x, y: s.frame.minY - s.gap - .random(in: 0...height))
        guard isVisible(point, on: s) else { return }
        spawnChunk(
            area: height * SnowSurface.dx,
            at: point,
            vel: CGVector(dx: s.snowVel.dx + .random(in: -18...18),
                          dy: s.winVel.dy + .random(in: -10...40)),
            landDelay: 0.05
        )
    }

    /// Breaks the whole drift into clumps that carry on with the drift's own momentum.
    private func detach(_ s: SnowSurface, spread: CGFloat, up: ClosedRange<CGFloat>, landDelay: TimeInterval) {
        guard s.hasSnow else { return }
        if s.onScreen {
            let dx = SnowSurface.dx
            var total: CGFloat = 0
            for h in s.heights { total += h }
            total *= dx
            let budget = CGFloat(max(40, min(500, maxChunks - chunks.count)))
            let unit = max(9, total / budget)
            var pending: CGFloat = 0
            for (i, h) in s.heights.enumerated() where h > 0.05 {
                pending += h * dx
                while pending >= unit {
                    pending -= unit
                    let point = CGPoint(
                        x: s.frame.minX + (CGFloat(i) + .random(in: 0...1)) * dx + s.shift,
                        y: s.frame.minY - s.gap - .random(in: 0...h)
                    )
                    guard isVisible(point, on: s) else { continue }
                    spawnChunk(
                        area: unit * .random(in: 0.6...1.4),
                        at: point,
                        vel: CGVector(dx: s.snowVel.dx + .random(in: -spread...spread),
                                      dy: s.snowVel.dy - .random(in: up)),
                        landDelay: landDelay
                    )
                }
            }
        }
        for i in s.heights.indices { s.heights[i] = 0 }
        s.hasSnow = false
        s.gap = 0
        s.shift = 0
        s.snowVel = s.winVel
        s.spillLeft = 0
        s.spillRight = 0
        s.dirty = true
    }

    /// Avalanching: snow can't hold a slope steeper than `maxStep` per column, and
    /// crumbles over the ends once it stands taller than `edgeCliff` there.
    private func relax(_ s: SnowSurface) {
        let n = s.heights.count
        guard n > 1, s.hasSnow else { return }
        var moved = false
        for i in 0..<(n - 1) {
            let d = s.heights[i] - s.heights[i + 1]
            if d > maxStep {
                let m = (d - maxStep) * 0.25
                s.heights[i] -= m; s.heights[i + 1] += m
                moved = true
            } else if d < -maxStep {
                let m = (-d - maxStep) * 0.25
                s.heights[i] += m; s.heights[i + 1] -= m
                moved = true
            }
        }

        let dx = SnowSurface.dx
        let lo = cornerInset, hi = s.frame.width - cornerInset
        if let first = (0..<n).first(where: { s.x(ofColumn: $0) >= lo }), s.heights[first] > edgeCliff {
            let m = (s.heights[first] - edgeCliff) * 0.25
            s.heights[first] -= m
            s.spillLeft += m * dx
            if s.spillLeft > 7 {
                spillOverEdge(s, localX: lo - 2, area: s.spillLeft, push: -1)
                s.spillLeft = 0
            }
            moved = true
        }
        if let last = (0..<n).last(where: { s.x(ofColumn: $0) <= hi }), s.heights[last] > edgeCliff {
            let m = (s.heights[last] - edgeCliff) * 0.25
            s.heights[last] -= m
            s.spillRight += m * dx
            if s.spillRight > 7 {
                spillOverEdge(s, localX: hi + 2, area: s.spillRight, push: 1)
                s.spillRight = 0
            }
            moved = true
        }
        // Keep settling on later frames until the slope is stable.
        if moved { s.dirty = true }
    }

    private func spillOverEdge(_ s: SnowSurface, localX: CGFloat, area: CGFloat, push: CGFloat) {
        guard s.onScreen else { return }
        let point = CGPoint(x: s.frame.minX + localX, y: s.frame.minY - s.gap - .random(in: 1...edgeCliff))
        guard isVisible(point, on: s) else { return }
        spawnChunk(area: area, at: point,
                   vel: CGVector(dx: s.winVel.dx + push * .random(in: 8...30), dy: s.winVel.dy),
                   landDelay: 0.05)
    }

    private func melt(elapsed: TimeInterval) {
        let keep = CGFloat(exp(-elapsed / meltTime))
        var dead: [SurfaceID] = []
        for (id, s) in surfaces {
            if s.hasSnow {
                var any = false
                for i in s.heights.indices {
                    let h = s.heights[i] * keep
                    if h < 0.05 { s.heights[i] = 0 } else { s.heights[i] = h; any = true }
                }
                s.hasSnow = any
                s.dirty = true
            }
            // Forget surfaces that are empty, or whose window has been gone for a long time.
            if !s.hasSnow || (!s.onScreen && clock - s.lastSeen > 600) {
                s.node?.removeFromParent()
                dead.append(id)
            }
        }
        for id in dead { surfaces[id] = nil }
    }

    private func isVisible(_ point: CGPoint, on s: SnowSurface) -> Bool {
        let tracker = WindowTracker.shared
        switch s.id {
        case let .window(id):
            guard let index = tracker.indexByID[id] else { return false }
            return tracker.isPointVisible(point, inFrontOf: index)
        case .dock:
            return tracker.isPointUncovered(point)
        }
    }

    // MARK: - Falling clumps

    private func spawnChunk(area: CGFloat, at point: CGPoint, vel: CGVector, landDelay: TimeInterval) {
        guard chunks.count < maxChunks, area > 0.5 else { return }
        let chunk = Chunk(node: dequeueNode())
        chunk.radius = max(1.2, min(7, sqrt(area / .pi)))
        chunk.pos = point
        chunk.vel = vel
        chunk.landAfter = landDelay
        chunk.alpha = 0.96
        configure(chunk)
        addChild(chunk.node)
        chunks.append(chunk)
    }

    private func spawnPowder(at point: CGPoint, vel: CGVector) {
        guard chunks.count < maxChunks else { return }
        let chunk = Chunk(node: dequeueNode())
        chunk.radius = .random(in: 0.9...1.9)
        chunk.pos = point
        chunk.vel = vel
        chunk.life = .random(in: 0.5...1.1)
        chunk.alpha = .random(in: 0.45...0.8)
        configure(chunk)
        addChild(chunk.node)
        chunks.append(chunk)
    }

    private func configure(_ chunk: Chunk) {
        let d = chunk.radius * 2 * 1.5
        chunk.node.size = CGSize(width: d, height: d)
        chunk.node.alpha = chunk.alpha
        chunk.node.zPosition = chunkZ
        chunk.node.position = scenePoint(chunk.pos)
    }

    /// Returns false when the chunk is gone (off screen, faded, or landed on a drift).
    private func stepChunk(_ c: Chunk, dt: CGFloat, wind: CGFloat, settleEnabled: Bool) -> Bool {
        c.age += Double(dt)
        let isPowder = c.life > 0
        if isPowder {
            if c.age >= c.life { return false }
            c.node.alpha = c.alpha * CGFloat(1 - c.age / c.life)
        }

        // Gravity against air drag. Bigger clumps have a higher terminal velocity,
        // so a falling sheet of snow strings out and breaks apart as it drops.
        let terminal: CGFloat = isPowder ? 70 : 260 + 110 * c.radius
        let drag = min(1, gravity / terminal * dt)
        c.vel.dy += gravity * dt
        c.vel.dy -= c.vel.dy * drag
        c.vel.dx += (wind * 0.5 - c.vel.dx) * drag

        let old = c.pos
        let new = CGPoint(x: old.x + c.vel.dx * dt, y: old.y + c.vel.dy * dt)
        c.pos = new

        if !isPowder {
            let speed = hypot(c.vel.dx, c.vel.dy)
            // Clumps crumble in flight and trail powder.
            if c.radius > 2.4, c.age > 0.08, speed > 180, CGFloat.random(in: 0...1) < 3 * dt {
                c.radius /= 1.414
                configure(c)
                let kick = CGFloat.random(in: 25...70) * (Bool.random() ? 1 : -1)
                spawnChunk(area: .pi * c.radius * c.radius, at: new,
                           vel: CGVector(dx: c.vel.dx + kick, dy: c.vel.dy * .random(in: 0.8...1.0)),
                           landDelay: max(0, c.landAfter - c.age))
                c.vel.dx -= kick * 0.5
            }
            if speed > 220, CGFloat.random(in: 0...1) < 1.2 * dt {
                spawnPowder(at: new, vel: CGVector(dx: c.vel.dx * 0.4 + .random(in: -30...30),
                                                   dy: c.vel.dy * 0.3))
            }

            if settleEnabled, c.vel.dy > 0, c.age >= c.landAfter,
               let (surface, localX) = landing(from: old, to: new, chance: 1) {
                deposit(surface, localX: localX, area: .pi * c.radius * c.radius * 0.85)
                if c.vel.dy > 300 {
                    // A hard landing throws up a little puff.
                    for _ in 0..<Int.random(in: 1...2) {
                        spawnPowder(at: CGPoint(x: new.x, y: new.y - 2),
                                    vel: CGVector(dx: .random(in: -90...90), dy: -.random(in: 40...150)))
                    }
                }
                return false
            }
        }

        if new.y > screenRect.maxY + 12 { return false }
        if new.x < screenRect.minX - 300 || new.x > screenRect.maxX + 300 { return false }
        return true
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
    private func stir(_ flake: Flake, dt: CGFloat) {
        guard let m = mouse else { return }
        let dx = flake.pos.x - m.x, dy = flake.pos.y - m.y
        let d2 = dx * dx + dy * dy
        guard d2 < pointerRadius * pointerRadius else { return }
        let d = max(sqrt(d2), 1)
        let w = 1 - d / pointerRadius
        let k = min(1, w * 10 * dt)
        flake.vel.dx += (mouseVel.dx * 0.6 - flake.vel.dx) * k + dx / d * w * 220 * dt
        flake.vel.dy += (mouseVel.dy * 0.6 - flake.vel.dy) * k + dy / d * w * 220 * dt
    }

    /// A fast pointer swipe through a drift ploughs a gap in it.
    private func carveWithPointer() {
        guard let m = mouse, hypot(mouseVel.dx, mouseVel.dy) > 250 else { return }
        let reach: CGFloat = 18
        for s in surfaces.values where s.hasSnow && s.onScreen {
            let top = s.frame.minY - s.gap
            guard m.y > top - maxHeight - 8, m.y < top + 8,
                  m.x > s.frame.minX - reach, m.x < s.frame.maxX + reach else { continue }
            let from = max(0, s.column(at: m.x - s.frame.minX - reach))
            let to = min(s.heights.count - 1, s.column(at: m.x - s.frame.minX + reach))
            guard from <= to else { continue }
            for i in from...to where s.heights[i] > 0.3 {
                let h = s.heights[i]
                guard m.y > top - h - 8 else { continue }
                let taken = h * 0.85
                s.heights[i] = h - taken
                let point = CGPoint(x: s.frame.minX + s.x(ofColumn: i), y: top - .random(in: 0...h))
                guard isVisible(point, on: s) else { continue }
                spawnChunk(
                    area: taken * SnowSurface.dx,
                    at: point,
                    vel: CGVector(dx: mouseVel.dx * 0.45 + .random(in: -50...50),
                                  dy: min(mouseVel.dy * 0.45, 0) - .random(in: 60...220)),
                    landDelay: 0.1
                )
                s.dirty = true
            }
        }
    }

    // MARK: - Drawing drifts

    private func rebuildCap(_ s: SnowSurface, tracker: WindowTracker) {
        s.dirty = false
        s.lastBuild = clock
        guard s.onScreen, s.hasSnow else {
            if !s.hasSnow { s.node?.path = nil }
            return
        }
        let frame = s.frame
        let dx = SnowSurface.dx
        let n = s.heights.count
        let windowIndex: Int?
        if case let .window(id) = s.id { windowIndex = tracker.indexByID[id] } else { windowIndex = nil }

        func visible(_ x: CGFloat, _ h: CGFloat) -> Bool {
            let edge = CGPoint(x: frame.minX + x, y: frame.minY + 1)
            let crest = CGPoint(x: frame.minX + x, y: frame.minY - s.gap - h)
            if let index = windowIndex {
                return tracker.isPointVisible(edge, inFrontOf: index)
                    && tracker.isPointVisible(crest, inFrontOf: index)
            }
            return tracker.isPointUncovered(crest)
        }

        // Trace the snow line left to right as a smooth curve, splitting it wherever
        // another window covers this one. Paths are in scene orientation (y up),
        // measured from the surface's top-left corner.
        let path = CGMutablePath()
        var run: [CGPoint] = []
        func flush() {
            guard let first = run.first, let last = run.last else { return }
            path.move(to: CGPoint(x: first.x - dx * 0.75, y: 0))
            var previous = CGPoint(x: first.x - dx * 0.75, y: 0)
            for p in run {
                path.addQuadCurve(to: CGPoint(x: (previous.x + p.x) / 2, y: (previous.y + p.y) / 2),
                                  control: previous)
                previous = p
            }
            let end = CGPoint(x: last.x + dx * 0.75, y: 0)
            path.addQuadCurve(to: CGPoint(x: (previous.x + end.x) / 2, y: (previous.y + end.y) / 2),
                              control: previous)
            path.addLine(to: end)
            path.closeSubpath()
            run.removeAll(keepingCapacity: true)
        }

        for i in 0..<n {
            let left = i > 0 ? s.heights[i - 1] : 0
            let right = i < n - 1 ? s.heights[i + 1] : 0
            let h = (left + 2 * s.heights[i] + right) / 4
            let x = s.x(ofColumn: i)
            if h > 0.3 && visible(x, h) {
                run.append(CGPoint(x: x, y: h))
            } else {
                flush()
            }
        }
        flush()

        if path.isEmpty {
            s.node?.path = nil
            return
        }
        let node = s.node ?? makeCapNode(for: s)
        node.path = path
    }

    private func makeCapNode(for s: SnowSurface) -> SKShapeNode {
        let node = SKShapeNode()
        node.fillColor = NSColor(white: 1, alpha: 0.97)
        // A faint outline keeps the snow visible on light window chrome.
        node.strokeColor = NSColor(white: 0.55, alpha: 0.35)
        node.lineWidth = 0.8
        node.isAntialiased = true
        node.zPosition = capZ
        addChild(node)
        s.node = node
        return node
    }

    // MARK: - Window events

    private func currentFrame(of id: SurfaceID, tracker: WindowTracker) -> CGRect? {
        switch id {
        case let .window(windowID):
            return tracker.framesByID[windowID]
        case let .dock(index):
            guard index < tracker.dockRects.count else { return nil }
            return tracker.dockRects[index]
        }
    }

    /// Called after every window poll: updates each drift's idea of where its window
    /// is and how fast it is moving.
    private func handle(_ events: [SurfaceEvent]) {
        let tracker = WindowTracker.shared
        var moved: [CGWindowID: CGVector] = [:]
        for event in events {
            switch event {
            case let .moved(id, velocity):
                moved[id] = velocity
            case let .relocated(id):
                // The window came back somewhere else; its old snow is gone.
                if let s = surfaces.removeValue(forKey: .window(id)) { s.node?.removeFromParent() }
            case .vanished:
                break // the drift stays, hidden, until the window returns
            }
        }

        for s in surfaces.values {
            guard let new = currentFrame(of: s.id, tracker: tracker) else {
                s.onScreen = false
                s.winVel = .zero
                continue
            }
            s.lastSeen = clock
            if !s.onScreen {
                s.onScreen = true
                s.winVel = .zero
                s.snowVel = .zero
                s.gap = 0
            }
            let old = s.frame
            s.frame = new

            var target = CGVector.zero
            if abs(new.width - old.width) > 0.5 {
                // Resizing isn't motion. If the left edge moved, the snow stays put
                // in the world, so it shifts within the window.
                if abs(new.maxX - old.maxX) < 0.5 {
                    s.shift -= new.minX - old.minX
                    normalizeShift(s)
                }
                resize(s, to: new.width)
                shedEdges(s)
            } else if case let .window(id) = s.id, let v = moved[id] {
                target = v
            }
            // Polls are jittery; smooth the measured velocity a little.
            s.winVel.dx += (target.dx - s.winVel.dx) * 0.6
            s.winVel.dy += (target.dy - s.winVel.dy) * 0.6
            if abs(s.winVel.dx) < 1 { s.winVel.dx = 0 }
            if abs(s.winVel.dy) < 1 { s.winVel.dy = 0 }

            // Occlusion may have changed even if nothing moved.
            s.dirty = true
        }
    }

    private func resize(_ s: SnowSurface, to width: CGFloat) {
        let n = SnowSurface.columns(for: width)
        if n < s.heights.count {
            for i in n..<s.heights.count where s.heights[i] > 0 {
                spill(from: s, column: i, height: s.heights[i])
            }
            s.heights.removeLast(s.heights.count - n)
        } else if n > s.heights.count {
            s.heights.append(contentsOf: repeatElement(0, count: n - s.heights.count))
        }
    }

    // MARK: - Coordinates

    private func scenePoint(_ global: CGPoint) -> CGPoint {
        CGPoint(x: global.x - screenRect.minX, y: screenRect.maxY - global.y)
    }
}
