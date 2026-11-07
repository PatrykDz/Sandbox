import AppKit
import ServiceManagement

@MainActor
final class AppDelegate: NSObject, NSApplicationDelegate, NSMenuDelegate {
    private var statusItem: NSStatusItem!
    private var overlays: [SnowOverlay] = []
    private var displaysAsleep = false
    private let settings = Settings.shared

    func applicationDidFinishLaunching(_ notification: Notification) {
        _ = PowerMonitor.shared
        setUpStatusItem()
        observeSystem()
        applySettings()
    }

    func applicationWillTerminate(_ notification: Notification) {
        removeOverlays()
    }

    // MARK: - Overlays

    private func applySettings() {
        if settings.enabled {
            if overlays.isEmpty { buildOverlays() }
        } else {
            removeOverlays()
        }
        updateStatusIcon()
    }

    private func buildOverlays() {
        removeOverlays()
        WindowTracker.shared.start()
        overlays = NSScreen.screens.map { SnowOverlay(screen: $0) }
        for overlay in overlays {
            overlay.setPaused(displaysAsleep)
            overlay.show()
        }
    }

    private func removeOverlays() {
        overlays.forEach { $0.tearDown() }
        overlays.removeAll()
        WindowTracker.shared.stop()
    }

    private func observeSystem() {
        let center = NotificationCenter.default
        center.addObserver(forName: Settings.didChange, object: nil, queue: .main) { [weak self] _ in
            MainActor.assumeIsolated { self?.applySettings() }
        }
        center.addObserver(forName: NSApplication.didChangeScreenParametersNotification, object: nil, queue: .main) { [weak self] _ in
            MainActor.assumeIsolated {
                guard let self, self.settings.enabled else { return }
                self.buildOverlays()
            }
        }
        center.addObserver(forName: PowerMonitor.didChange, object: nil, queue: .main) { [weak self] _ in
            MainActor.assumeIsolated { self?.overlays.forEach { $0.powerStateChanged() } }
        }

        let workspace = NSWorkspace.shared.notificationCenter
        workspace.addObserver(forName: NSWorkspace.screensDidSleepNotification, object: nil, queue: .main) { [weak self] _ in
            MainActor.assumeIsolated { self?.setDisplaysAsleep(true) }
        }
        workspace.addObserver(forName: NSWorkspace.screensDidWakeNotification, object: nil, queue: .main) { [weak self] _ in
            MainActor.assumeIsolated { self?.setDisplaysAsleep(false) }
        }
    }

    private func setDisplaysAsleep(_ asleep: Bool) {
        displaysAsleep = asleep
        overlays.forEach { $0.setPaused(asleep) }
        if asleep {
            WindowTracker.shared.stop()
        } else if settings.enabled {
            WindowTracker.shared.start()
        }
    }

    // MARK: - Menu bar

    private func setUpStatusItem() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)
        let menu = NSMenu()
        menu.delegate = self
        statusItem.menu = menu
        updateStatusIcon()
    }

    private func updateStatusIcon() {
        let name = settings.enabled ? "snowflake" : "snowflake.slash"
        let image = NSImage(systemSymbolName: name, accessibilityDescription: "Snowfall")
            ?? NSImage(systemSymbolName: "snowflake", accessibilityDescription: "Snowfall")
        image?.isTemplate = true
        statusItem?.button?.image = image
    }

    func menuNeedsUpdate(_ menu: NSMenu) {
        menu.removeAllItems()

        let toggle = item("Let It Snow", #selector(toggleSnow))
        toggle.state = settings.enabled ? .on : .off
        menu.addItem(toggle)
        menu.addItem(.separator())

        let intensityMenu = NSMenu()
        for intensity in Intensity.allCases {
            let entry = item(intensity.title, #selector(chooseIntensity(_:)))
            entry.tag = intensity.rawValue
            entry.state = settings.intensity == intensity ? .on : .off
            intensityMenu.addItem(entry)
        }
        let intensityItem = NSMenuItem(title: "Intensity", action: nil, keyEquivalent: "")
        intensityItem.submenu = intensityMenu
        menu.addItem(intensityItem)

        let windMenu = NSMenu()
        for wind in Wind.allCases {
            let entry = item(wind.title, #selector(chooseWind(_:)))
            entry.tag = wind.rawValue
            entry.state = settings.wind == wind ? .on : .off
            windMenu.addItem(entry)
        }
        let windItem = NSMenuItem(title: "Wind", action: nil, keyEquivalent: "")
        windItem.submenu = windMenu
        menu.addItem(windItem)

        let settle = item("Pile Up on Windows & Dock", #selector(toggleSettle))
        settle.state = settings.settle ? .on : .off
        menu.addItem(settle)

        let clear = item("Shake Off Settled Snow", #selector(clearSnow))
        clear.isEnabled = settings.enabled
        menu.addItem(clear)

        if PowerMonitor.shared.isConstrained {
            let note = NSMenuItem(title: "Reduced for battery / Low Power Mode", action: nil, keyEquivalent: "")
            note.isEnabled = false
            menu.addItem(note)
        }

        menu.addItem(.separator())

        let login = item("Launch at Login", #selector(toggleLaunchAtLogin))
        login.state = SMAppService.mainApp.status == .enabled ? .on : .off
        menu.addItem(login)

        menu.addItem(.separator())
        let quit = NSMenuItem(title: "Quit Snowfall", action: #selector(NSApplication.terminate(_:)), keyEquivalent: "q")
        menu.addItem(quit)
    }

    private func item(_ title: String, _ action: Selector) -> NSMenuItem {
        let item = NSMenuItem(title: title, action: action, keyEquivalent: "")
        item.target = self
        return item
    }

    @objc private func toggleSnow() {
        settings.enabled.toggle()
    }

    @objc private func chooseIntensity(_ sender: NSMenuItem) {
        if let value = Intensity(rawValue: sender.tag) { settings.intensity = value }
    }

    @objc private func chooseWind(_ sender: NSMenuItem) {
        if let value = Wind(rawValue: sender.tag) { settings.wind = value }
    }

    @objc private func toggleSettle() {
        settings.settle.toggle()
    }

    @objc private func clearSnow() {
        overlays.forEach { $0.clearSettledSnow() }
    }

    @objc private func toggleLaunchAtLogin() {
        let service = SMAppService.mainApp
        do {
            if service.status == .enabled {
                try service.unregister()
            } else {
                try service.register()
            }
        } catch {
            let alert = NSAlert()
            alert.messageText = "Couldn't change the login item"
            alert.informativeText = "\(error.localizedDescription)\n\nLaunch at Login works when Snowfall runs from the bundled Snowfall.app (see build-app.sh)."
            NSApp.activate()
            alert.runModal()
        }
    }
}
