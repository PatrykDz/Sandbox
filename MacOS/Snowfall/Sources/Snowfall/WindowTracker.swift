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
    /// The window moved or changed width; anything resting on it should slide off.
    case moved(CGWindowID, velocity: CGVector)
    /// The window closed, minimized or left this Space; resting snow melts away.
    case vanished(CGWindowID)
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
    /// Rects of the Dock's window(s), if it is visible.
    private(set) var dockRects: [CGRect] = []

    private var observers: [UUID: ([SurfaceEvent]) -> Void] = [:]
    private var timer: Timer?
    private var lastPoll: CFTimeInterval = 0
    private let pollInterval: TimeInterval = 1.0 / 15.0
    private let ownPID = ProcessInfo.processInfo.processIdentifier
    private let dockLevel = Int(CGWindowLevelForKey(.dockWindow))

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
        poll()
        let timer = Timer(timeInterval: pollInterval, repeats: true) { _ in
            MainActor.assumeIsolated { WindowTracker.shared.poll() }
        }
        // Keep polling while menus are open or windows are being dragged.
        RunLoop.main.add(timer, forMode: .common)
        self.timer = timer
    }

    func stop() {
        timer?.invalidate()
        timer = nil
    }

    /// True when `point` is not covered by any window in front of the window at `index`.
    func isPointVisible(_ point: CGPoint, inFrontOf index: Int) -> Bool {
        var i = 0
        while i < index && i < windows.count {
            if windows[i].frame.contains(point) { return false }
            i += 1
        }
        return true
    }

    /// True when `point` is not covered by any tracked window at all.
    func isPointUncovered(_ point: CGPoint) -> Bool {
        isPointVisible(point, inFrontOf: windows.count)
    }

    func index(of id: CGWindowID) -> Int? {
        windows.firstIndex { $0.id == id }
    }

    private func poll() {
        guard let info = CGWindowListCopyWindowInfo(
            [.optionOnScreenOnly, .excludeDesktopElements], kCGNullWindowID
        ) as? [[String: Any]] else { return }

        let now = CACurrentMediaTime()
        let dt = lastPoll == 0 ? pollInterval : max(now - lastPoll, 1.0 / 120.0)
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

            if pid == Int(ownPID) { continue }
            let alpha = entry[kCGWindowAlpha as String] as? Double ?? 1
            if alpha < 0.05 { continue }

            let owner = entry[kCGWindowOwnerName as String] as? String
            if owner == "Dock" && layer == dockLevel {
                newDocks.append(bounds)
                continue
            }

            guard layer == 0, bounds.width >= 80, bounds.height >= 40 else { continue }
            newWindows.append(TrackedWindow(id: CGWindowID(truncatingIfNeeded: number), frame: bounds))
        }

        var newFrames: [CGWindowID: CGRect] = [:]
        newFrames.reserveCapacity(newWindows.count)
        var events: [SurfaceEvent] = []

        for window in newWindows {
            newFrames[window.id] = window.frame
            guard let old = framesByID[window.id] else { continue }
            let f = window.frame
            if old.minX != f.minX || old.minY != f.minY || old.width != f.width {
                let velocity = CGVector(dx: (f.minX - old.minX) / dt, dy: (f.minY - old.minY) / dt)
                events.append(.moved(window.id, velocity: velocity))
            }
        }
        for id in framesByID.keys where newFrames[id] == nil {
            events.append(.vanished(id))
        }

        windows = newWindows
        framesByID = newFrames
        dockRects = newDocks

        for handler in observers.values {
            handler(events)
        }
    }
}
