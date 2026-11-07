import Foundation
import CoreGraphics

enum Intensity: Int, CaseIterable {
    case light, moderate, blizzard

    var title: String {
        switch self {
        case .light: return "Light"
        case .moderate: return "Moderate"
        case .blizzard: return "Blizzard"
        }
    }

    /// New flakes per second for every 1000 points of screen width.
    var spawnRate: Double {
        switch self {
        case .light: return 18
        case .moderate: return 45
        case .blizzard: return 120
        }
    }

    /// Upper bound on airborne flakes per screen.
    var maxFalling: Int {
        switch self {
        case .light: return 350
        case .moderate: return 800
        case .blizzard: return 1800
        }
    }
}

enum Wind: Int, CaseIterable {
    case calm, breeze, gusty

    var title: String {
        switch self {
        case .calm: return "Calm"
        case .breeze: return "Breeze"
        case .gusty: return "Gusty"
        }
    }

    /// Base horizontal drift in points per second.
    var strength: CGFloat {
        switch self {
        case .calm: return 0
        case .breeze: return 28
        case .gusty: return 75
        }
    }
}

@MainActor
final class Settings {
    static let shared = Settings()
    static let didChange = Notification.Name("SnowfallSettingsDidChange")

    private let defaults = UserDefaults.standard

    private init() {
        defaults.register(defaults: [
            Keys.enabled: true,
            Keys.intensity: Intensity.moderate.rawValue,
            Keys.wind: Wind.breeze.rawValue,
            Keys.settle: true,
        ])
    }

    var enabled: Bool {
        get { defaults.bool(forKey: Keys.enabled) }
        set { defaults.set(newValue, forKey: Keys.enabled); notify() }
    }

    var intensity: Intensity {
        get { Intensity(rawValue: defaults.integer(forKey: Keys.intensity)) ?? .moderate }
        set { defaults.set(newValue.rawValue, forKey: Keys.intensity); notify() }
    }

    var wind: Wind {
        get { Wind(rawValue: defaults.integer(forKey: Keys.wind)) ?? .breeze }
        set { defaults.set(newValue.rawValue, forKey: Keys.wind); notify() }
    }

    /// Whether snow piles up on window tops and the Dock.
    var settle: Bool {
        get { defaults.bool(forKey: Keys.settle) }
        set { defaults.set(newValue, forKey: Keys.settle); notify() }
    }

    private func notify() {
        NotificationCenter.default.post(name: Settings.didChange, object: self)
    }

    private enum Keys {
        static let enabled = "enabled"
        static let intensity = "intensity"
        static let wind = "wind"
        static let settle = "settle"
    }
}
