import AppKit
import CoreGraphics
import QuartzCore

/// An on-screen window that snow can land on.
/// `frame` is in global CoreGraphics coordinates (origin at the top-left of the
/// primary display, y grows downward).
struct TrackedWindow {
    let id: CGWindowID
    let frame: CGRect
}

enum SurfaceEvent {
    /// The window moved or changed width while on screen; snow on it should slide off.
    case moved(CGWindowID, velocity: CGVector)
    /// The window left the screen (closed, minimized, other Space, Mission Control...).
    /// Its snow is kept hidden in case it comes back.
    case vanished(CGWindowID)
    /// A window that had vanished came back somewhere else; its old snow is gone.
    case relocated(CGWindowID)
}

/// Polls the window server for the positions of on-screen windows so the snow
/// simulation knows where window tops and the Dock are.
///
/// Reading window bounds via `CGWindowListCopyWindowInfo` does not require the
/// Screen Recording permission (only window titles do), so no prompt appears.
@MainActor
final class WindowTracker {
    static let shared = WindowTracker()

    /// Normal app windows, ordered front to back.
    private(set) var windows: [TrackedWindow] = []
    private(set) var framesByID: [CGWindowID: CGRect] = [:]
    /// Front-to-back position of each window in `windows`.
    private(set) var indexByID: [CGWindowID: Int] = [:]
    /// Rects of the Dock's window(s), if it is visible.
    private(set) var dockRects: [CGRect] = []

    private(set) var isRunning = false
    private var observers: [UUID: ([SurfaceEvent]) -> Void] = [:]
    private var timer: Timer?
    private var lastPoll: CFTimeInterval = 0
    /// Last frame of windows that left the screen, so we can tell if they return unchanged.
    private var parked: [CGWindowID: (frame: CGRect, since: CFTimeInterval)] = [:]
    private let parkedLifetime: CFTimeInterval = 600
    private let pollInterval: TimeInterval = 1.0 / 15.0
    private var tick = 0
    private var fastUntil: CFTimeInterval = 0
    private let ownPID = Int(ProcessInfo.processInfo.processIdentifier)
    private let dockLevel = Int(CGWindowLevelForKey(.dockWindow))
    /// System processes whose normal-level windows are not real app windows.
    private let ignoredOwners: Set<String> = [
        "Window Server", "Dock", "Control Center", "Notification Center",
        "SystemUIServer", "Spotlight", "screencaptureui", "Screenshot",
    ]

    private init() {}

    func addObserver(_ handler: @escaping ([SurfaceEvent]) -> Void) -> UUID {
        let token = UUID()
        observers[token] = handler
        return token
    }

    func removeObserver(_ token: UUID) {
        observers.removeValue(forKey: token)
    }

    func start() {
        guard timer == nil else { return }
        isRunning = true
        lastPoll = 0
        tick = 0
        poll()
        // Ticks at 60 Hz, but only polls every 4th tick unless a window moved recently:
        // idle cost stays low while drags are tracked smoothly.
        let timer = Timer(timeInterval: 1.0 / 60.0, repeats: true) { _ in
            MainActor.assumeIsolated { WindowTracker.shared.timerFired() }
        }
        // Keep polling while menus are open or windows are being dragged.
        RunLoop.main.add(timer, forMode: .common)
        self.timer = timer
    }

    private func timerFired() {
        tick &+= 1
        if CACurrentMediaTime() < fastUntil || tick % 4 == 0 { poll() }
    }

    func stop() {
        timer?.invalidate()
        timer = nil
        isRunning = false
    }

    /// True when `point` is not covered by any window in front of the window at `index`.
    func isPointVisible(_ point: CGPoint, inFrontOf index: Int) -> Bool {
        let end = min(index, windows.count)
        var i = 0
        while i < end {
            if windows[i].frame.contains(point) { return false }
            i += 1
        }
        return true
    }

    /// True when `point` is not covered by any tracked window at all.
    func isPointUncovered(_ point: CGPoint) -> Bool {
        isPointVisible(point, inFrontOf: windows.count)
    }

    private func poll() {
        guard let info = CGWindowListCopyWindowInfo(
            [.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID
        ) as? [[String: Any]] else { return }

        let now = CACurrentMediaTime()
        let dt = lastPoll == 0 ? pollInterval : max(now - lastPoll, 1.0 / 120.0)
        // A long gap (e.g. after a pause) would give meaningless velocities.
        let velocityScale: CGFloat = (lastPoll == 0 || now - lastPoll > 0.5) ? 0 : 1
        lastPoll = now

        var newWindows: [TrackedWindow] = []
        var newDocks: [CGRect] = []

        for entry in info {
            guard let number = entry[kCGWindowNumber as String] as? Int,
                  let pid = entry[kCGWindowOwnerPID as String] as? Int,
                  let layer = entry[kCGWindowLayer as String] as? Int,
                  let boundsDict = entry[kCGWindowBounds as String] as? NSDictionary,
                  let bounds = CGRect(dictionaryRepresentation: boundsDict as CFDictionary)
            else { continue }

            if pid == ownPID { continue }
            let alpha = entry[kCGWindowAlpha as String] as? Double ?? 1
            if alpha < 0.05 { continue }

            let owner = entry[kCGWindowOwnerName as String] as? String ?? ""
            if owner == "Dock" && layer == dockLevel {
                newDocks.append(bounds)
                continue
            }

            guard layer == 0, bounds.width >= 80, bounds.height >= 40,
                  !ignoredOwners.contains(owner) else { continue }
            newWindows.append(TrackedWindow(id: CGWindowID(truncatingIfNeeded: number), frame: bounds))
        }

        var newFrames: [CGWindowID: CGRect] = [:]
        var newIndex: [CGWindowID: Int] = [:]
        newFrames.reserveCapacity(newWindows.count)
        newIndex.reserveCapacity(newWindows.count)
        var events: [SurfaceEvent] = []

        for (i, window) in newWindows.enumerated() {
            let f = window.frame
            newFrames[window.id] = f
            newIndex[window.id] = i
            if let old = framesByID[window.id] {
                if old.minX != f.minX || old.minY != f.minY || old.width != f.width {
                    let velocity = CGVector(
                        dx: (f.minX - old.minX) / dt * velocityScale,
                        dy: (f.minY - old.minY) / dt * velocityScale
                    )
                    events.append(.moved(window.id, velocity: velocity))
                }
            } else if let back = parked.removeValue(forKey: window.id) {
                if back.frame.minX != f.minX || back.frame.minY != f.minY || back.frame.width != f.width {
                    events.append(.relocated(window.id))
                }
            }
        }
        for (id, frame) in framesByID where newFrames[id] == nil {
            events.append(.vanished(id))
            parked[id] = (frame, now)
        }
        parked = parked.filter { now - $0.value.since < parkedLifetime }

        if events.contains(where: { if case .moved = $0 { return true } else { return false } }) {
            fastUntil = now + 0.6
        }

        windows = newWindows
        framesByID = newFrames
        indexByID = newIndex
        dockRects = newDocks

        for handler in observers.values {
            handler(events)
        }
    }
}
