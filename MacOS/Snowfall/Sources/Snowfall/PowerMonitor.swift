import AppKit
import IOKit.ps

/// Watches battery, Low Power Mode and the Reduce Motion accessibility setting so the
/// snow can be scaled back.
@MainActor
final class PowerMonitor {
    static let shared = PowerMonitor()
    static let didChange = Notification.Name("SnowfallPowerDidChange")

    /// On battery or in Low Power Mode.
    private(set) var isConstrained = false
    private(set) var reduceMotion = false
    private var timer: Timer?

    /// Multiplier applied to snow density.
    var densityFactor: Double { (isConstrained ? 0.5 : 1.0) * (reduceMotion ? 0.5 : 1.0) }
    var preferredFPS: Int { isConstrained ? 30 : 60 }

    private init() {
        refresh()
        NotificationCenter.default.addObserver(
            forName: .NSProcessInfoPowerStateDidChange, object: nil, queue: .main
        ) { _ in
            MainActor.assumeIsolated { PowerMonitor.shared.refresh() }
        }
        NSWorkspace.shared.notificationCenter.addObserver(
            forName: NSWorkspace.accessibilityDisplayOptionsDidChangeNotification, object: nil, queue: .main
        ) { _ in
            MainActor.assumeIsolated { PowerMonitor.shared.refresh() }
        }
        // Power source changes are infrequent; a slow poll is plenty.
        timer = Timer.scheduledTimer(withTimeInterval: 20, repeats: true) { _ in
            MainActor.assumeIsolated { PowerMonitor.shared.refresh() }
        }
    }

    func refresh() {
        let constrained = ProcessInfo.processInfo.isLowPowerModeEnabled || Self.isOnBattery()
        let reduce = NSWorkspace.shared.accessibilityDisplayShouldReduceMotion
        if constrained != isConstrained || reduce != reduceMotion {
            isConstrained = constrained
            reduceMotion = reduce
            NotificationCenter.default.post(name: Self.didChange, object: self)
        }
    }

    private static func isOnBattery() -> Bool {
        guard let info = IOPSCopyPowerSourcesInfo()?.takeRetainedValue(),
              let type = IOPSGetProvidingPowerSourceType(info)?.takeUnretainedValue()
        else { return false }
        return (type as String) == kIOPMBatteryPowerKey
    }
}
