import Foundation
import IOKit.ps

/// Watches Low Power Mode and battery state so the snow can be scaled back.
@MainActor
final class PowerMonitor {
    static let shared = PowerMonitor()
    static let didChange = Notification.Name("SnowfallPowerDidChange")

    private(set) var isConstrained = false
    private var timer: Timer?

    /// Multiplier applied to snow density: half on battery or in Low Power Mode.
    var densityFactor: Double { isConstrained ? 0.5 : 1.0 }
    var preferredFPS: Int { isConstrained ? 30 : 60 }

    private init() {
        refresh()
        NotificationCenter.default.addObserver(
            forName: .NSProcessInfoPowerStateDidChange, object: nil, queue: .main
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
        if constrained != isConstrained {
            isConstrained = constrained
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
