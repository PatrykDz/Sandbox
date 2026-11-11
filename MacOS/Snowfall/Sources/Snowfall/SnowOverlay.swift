import AppKit
import SpriteKit

/// A transparent, click-through window covering one display.
final class OverlayWindow: NSWindow {
    init(screen: NSScreen) {
        super.init(contentRect: screen.frame, styleMask: .borderless, backing: .buffered, defer: false)
        setFrame(screen.frame, display: false)
        isOpaque = false
        backgroundColor = .clear
        hasShadow = false
        ignoresMouseEvents = true
        isReleasedWhenClosed = false
        animationBehavior = .none
        // Just above the Dock so snow can rest on it, but below the menu bar and menus.
        level = NSWindow.Level(rawValue: Int(CGWindowLevelForKey(.dockWindow)) + 1)
        // Follow the user across Spaces. Deliberately *not* .fullScreenAuxiliary, so the
        // overlay is absent from full-screen apps and pauses there.
        collectionBehavior = [.canJoinAllSpaces, .stationary, .ignoresCycle]
    }

    override var canBecomeKey: Bool { false }
    override var canBecomeMain: Bool { false }

    // Borderless windows are normally pushed below the menu bar; keep the full screen.
    override func constrainFrameRect(_ frameRect: NSRect, to screen: NSScreen?) -> NSRect {
        frameRect
    }
}

/// Owns the overlay window, SpriteKit view and scene for one display.
@MainActor
final class SnowOverlay {
    let screen: NSScreen
    /// Called when the overlay starts or stops rendering.
    var onRunningChange: (() -> Void)?
    private(set) var isRunning = false

    private let window: OverlayWindow
    private let skView: SKView
    private let scene: SnowScene
    private var occlusionObserver: NSObjectProtocol?
    private var externallyPaused = false

    init(screen: NSScreen) {
        self.screen = screen
        window = OverlayWindow(screen: screen)

        skView = SKView(frame: NSRect(origin: .zero, size: screen.frame.size))
        skView.allowsTransparency = true
        skView.ignoresSiblingOrder = true
        skView.shouldCullNonVisibleNodes = false
        skView.preferredFramesPerSecond = PowerMonitor.shared.preferredFPS
        skView.autoresizingMask = [.width, .height]

        let primaryHeight = NSScreen.screens.first?.frame.height ?? screen.frame.height
        scene = SnowScene(screenRect: Self.globalRect(of: screen, primaryHeight: primaryHeight),
                          primaryHeight: primaryHeight)
        window.contentView = skView
        skView.presentScene(scene)

        occlusionObserver = NotificationCenter.default.addObserver(
            forName: NSWindow.didChangeOcclusionStateNotification, object: window, queue: .main
        ) { [weak self] _ in
            MainActor.assumeIsolated { self?.updatePaused() }
        }
    }

    func show() {
        window.orderFrontRegardless()
        updatePaused()
    }

    func tearDown() {
        onRunningChange = nil
        if let observer = occlusionObserver {
            NotificationCenter.default.removeObserver(observer)
            occlusionObserver = nil
        }
        skView.presentScene(nil)
        window.orderOut(nil)
        window.close()
    }

    func setPaused(_ paused: Bool) {
        externallyPaused = paused
        updatePaused()
    }

    func powerStateChanged() {
        skView.preferredFramesPerSecond = PowerMonitor.shared.preferredFPS
    }

    func shakeOffSettledSnow() {
        scene.shakeOffSettledSnow()
    }

    /// Pause when the display sleeps, or when the overlay isn't visible
    /// (e.g. a full-screen app owns this display).
    private func updatePaused() {
        let visible = window.occlusionState.contains(.visible)
        let running = !externallyPaused && visible
        skView.isPaused = !running
        if running != isRunning {
            isRunning = running
            onRunningChange?()
        }
    }

    /// The screen's frame converted to global CoreGraphics coordinates
    /// (top-left origin on the primary display, y down), matching window bounds.
    static func globalRect(of screen: NSScreen, primaryHeight: CGFloat) -> CGRect {
        let f = screen.frame
        return CGRect(x: f.minX, y: primaryHeight - f.maxY, width: f.width, height: f.height)
    }
}
