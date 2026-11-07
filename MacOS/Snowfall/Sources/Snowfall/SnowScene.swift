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
        case melting(total: CGFloat, remaining: CGFloat)
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
    var age: TimeInterval = 0
    var restUntil: TimeInterval = 0
    var noSettleUntil: TimeInterval = 0
    var state: State = .falling

    init(node: SKSpriteNode) { self.node = node }

    var canSettle: Bool { depth > 0.4 }
}

/// One scene per display. All physics runs in global CoreGraphics coordinates so
/// flakes line up with the window frames reported by `WindowTracker`.
@MainActor
final class SnowScene: SKScene {
    /// The display's rect in global CoreGraphics coordinates.
    var screenRect: CGRect

    private var flakes: [Flake] = []
    private var pool: [SKSpriteNode] = []
    private var spawnAccumulator: Double = 0
    private var lastUpdate: TimeInterval = 0
    private var clock: TimeInterval = 0
    private var restingCount = 0
    private var piles: [SurfaceID: [Int: CGFloat]] = [:]
    private var trackerToken: UUID?

    private let maxResting = 1400
    private let pileBucket: CGFloat = 4
    private let maxPileHeight: CGFloat = 16
    private let settleChance: CGFloat = 0.55

    private static let texture: SKTexture = {
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
        let texture = SKTexture(image: image)
        texture.filteringMode = .linear
        return texture
    }()

    init(screenRect: CGRect) {
        self.screenRect = screenRect
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
    }

    override func willMove(from view: SKView) {
        if let token = trackerToken {
            WindowTracker.shared.removeObserver(token)
            trackerToken = nil
        }
    }

    // MARK: - Public controls

    /// Melts every flake resting anywhere.
    func clearSettledSnow() {
        for flake in flakes {
            if case .resting = flake.state { melt(flake, over: 0.8) }
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
        let power = PowerMonitor.shared

        spawn(dt: dt, settings: settings, density: power.densityFactor)

        let windTarget = currentWind(settings.wind)
        let tracker = WindowTracker.shared
        let settleEnabled = settings.settle

        var i = 0
        while i < flakes.count {
            let flake = flakes[i]
            flake.age += dt
            var alive = true

            switch flake.state {
            case .falling:
                alive = stepFalling(flake, dt: dt, wind: windTarget, settleEnabled: settleEnabled)

            case let .resting(surface, offset):
                if let base = origin(of: surface, tracker: tracker) {
                    flake.pos = CGPoint(x: base.x + offset.x, y: base.y + offset.y)
                    if !settleEnabled {
                        release(flake, velocity: .zero)
                    } else if flake.age > flake.restUntil {
                        melt(flake, over: 3)
                    }
                } else {
                    melt(flake, over: 0.6)
                }

            case let .melting(total, remaining):
                let left = remaining - CGFloat(dt)
                if left <= 0 {
                    alive = false
                } else {
                    flake.state = .melting(total: total, remaining: left)
                    let t = left / total
                    flake.node.alpha = flake.baseAlpha * t
                    flake.node.setScale(0.6 + 0.4 * t)
                }
            }

            if alive {
                flake.node.position = scenePoint(flake.pos)
                i += 1
            } else {
                remove(at: i)
            }
        }
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
        let falling = flakes.count - restingCount
        let cap = Int(Double(intensity.maxFalling) * density)
        spawnAccumulator += intensity.spawnRate * density * Double(screenRect.width) / 1000 * dt
        while spawnAccumulator >= 1 {
            spawnAccumulator -= 1
            if falling >= cap { continue }
            spawnFlake(windStrength: settings.wind.strength)
        }
    }

    private func spawnFlake(windStrength: CGFloat) {
        let node = pool.popLast() ?? {
            let n = SKSpriteNode(texture: Self.texture)
            n.blendMode = .alpha
            return n
        }()

        let flake = Flake(node: node)
        // Bias towards distant flakes for a sense of depth.
        flake.depth = pow(CGFloat.random(in: 0...1), 0.8)
        flake.radius = 0.9 + flake.depth * 3.1
        flake.baseAlpha = 0.4 + 0.6 * flake.depth
        flake.wobbleAmp = 6 + 16 * flake.depth
        flake.wobbleFreq = .random(in: 0.6...1.6)
        flake.phase = .random(in: 0...(2 * .pi))

        // Spawn a little upwind so the left edge isn't bare in a breeze.
        let upwind = windStrength * 3
        let x = CGFloat.random(in: (screenRect.minX - upwind)...(screenRect.maxX + 20))
        flake.pos = CGPoint(x: x, y: screenRect.minY - CGFloat.random(in: 4...30))
        flake.vel = CGVector(dx: 0, dy: terminalSpeed(flake))

        let diameter = flake.radius * 2 * 1.5
        node.size = CGSize(width: diameter, height: diameter)
        node.alpha = flake.baseAlpha
        node.setScale(1)
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

        let margin: CGFloat = 60
        if new.y > screenRect.maxY + 10 { return false }
        if new.x < screenRect.minX - margin * 4 || new.x > screenRect.maxX + margin * 4 { return false }
        return true
    }

    private func tryLand(_ flake: Flake, from old: CGPoint, to new: CGPoint) -> Bool {
        let tracker = WindowTracker.shared

        for (index, window) in tracker.windows.enumerated() {
            let frame = window.frame
            guard new.x > frame.minX + 3, new.x < frame.maxX - 3 else { continue }
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
            guard new.x > dock.minX + 2, new.x < dock.maxX - 2 else { continue }
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

    private func isPlausibleDock(_ rect: CGRect) -> Bool {
        rect.intersects(screenRect) && rect.height < screenRect.height * 0.35 && rect.maxY >= screenRect.maxY - 4
    }

    private func settle(_ flake: Flake, on surface: SurfaceID, base: CGPoint, top: CGFloat) {
        let restY = top - flake.radius * 0.45
        flake.pos.y = restY
        flake.vel = .zero
        let offset = CGPoint(x: flake.pos.x - base.x, y: restY - base.y)
        flake.state = .resting(surface, offset: offset)
        flake.restUntil = flake.age + .random(in: 40...120)
        flake.node.alpha = min(1, flake.baseAlpha + 0.15)
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
        flake.node.alpha = flake.baseAlpha
    }

    private func melt(_ flake: Flake, over duration: CGFloat) {
        switch flake.state {
        case let .resting(surface, offset):
            adjustPile(surface, x: offset.x, radius: flake.radius, sign: -1)
            restingCount -= 1
        case .melting:
            return
        case .falling:
            break
        }
        flake.state = .melting(total: duration, remaining: duration)
        flake.baseAlpha = flake.node.alpha
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

    // MARK: - Piles

    private func pileHeight(_ surface: SurfaceID, x: CGFloat) -> CGFloat {
        piles[surface]?[Int(x / pileBucket)] ?? 0
    }

    private func adjustPile(_ surface: SurfaceID, x: CGFloat, radius: CGFloat, sign: CGFloat) {
        let lo = Int((x - radius) / pileBucket)
        let hi = Int((x + radius) / pileBucket)
        var buckets = piles[surface] ?? [:]
        for b in lo...hi {
            let h = (buckets[b] ?? 0) + sign * radius * 0.5
            buckets[b] = min(maxPileHeight, max(0, h))
        }
        piles[surface] = buckets
    }

    // MARK: - Window events

    private func origin(of surface: SurfaceID, tracker: WindowTracker) -> CGPoint? {
        switch surface {
        case let .window(id):
            return tracker.framesByID[id]?.origin
        case let .dock(index):
            guard index < tracker.dockRects.count else { return nil }
            return tracker.dockRects[index].origin
        }
    }

    private func handle(_ events: [SurfaceEvent]) {
        if !events.isEmpty {
            var moved: [CGWindowID: CGVector] = [:]
            var vanished = Set<CGWindowID>()
            for event in events {
                switch event {
                case let .moved(id, velocity): moved[id] = velocity
                case let .vanished(id): vanished.insert(id)
                }
            }

            for flake in flakes {
                guard case let .resting(.window(id), _) = flake.state else { continue }
                if let v = moved[id] {
                    // The window was yanked out from under the snow: fling it.
                    let fling = CGVector(
                        dx: clamp(v.dx * 0.55, 1400) + .random(in: -40...40),
                        dy: min(clamp(v.dy * 0.55, 1400), 0) - .random(in: 15...90)
                    )
                    release(flake, velocity: fling)
                } else if vanished.contains(id) {
                    melt(flake, over: 0.5)
                }
            }
            for id in moved.keys { piles[.window(id)] = nil }
            for id in vanished { piles[.window(id)] = nil }
        }

        // Snow on a window that is now covered by another one quietly melts.
        let tracker = WindowTracker.shared
        for flake in flakes {
            guard case let .resting(.window(id), _) = flake.state,
                  let index = tracker.index(of: id) else { continue }
            if !tracker.isPointVisible(flake.pos, inFrontOf: index) {
                melt(flake, over: 0.4)
            }
        }
    }

    private func clamp(_ value: CGFloat, _ limit: CGFloat) -> CGFloat {
        max(-limit, min(limit, value))
    }

    // MARK: - Coordinates

    private func scenePoint(_ global: CGPoint) -> CGPoint {
        CGPoint(x: global.x - screenRect.minX, y: screenRect.maxY - global.y)
    }
}
