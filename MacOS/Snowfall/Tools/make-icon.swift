// Renders the 1024×1024 app icon PNG. Usage: swift Tools/make-icon.swift out.png
import AppKit

let px = 1024
guard let rep = NSBitmapImageRep(
    bitmapDataPlanes: nil, pixelsWide: px, pixelsHigh: px, bitsPerSample: 8,
    samplesPerPixel: 4, hasAlpha: true, isPlanar: false, colorSpaceName: .deviceRGB,
    bytesPerRow: 0, bitsPerPixel: 0
) else { fatalError("bitmap") }
rep.size = NSSize(width: px, height: px)

NSGraphicsContext.saveGraphicsState()
NSGraphicsContext.current = NSGraphicsContext(bitmapImageRep: rep)

let canvas = NSRect(x: 0, y: 0, width: px, height: px)
// macOS icon grid: 824pt body centred on a 1024 canvas.
let body = canvas.insetBy(dx: 100, dy: 100)
let shape = NSBezierPath(roundedRect: body, xRadius: 185, yRadius: 185)

// Night-sky gradient.
NSGradient(colors: [
    NSColor(srgbRed: 0.42, green: 0.62, blue: 0.90, alpha: 1),
    NSColor(srgbRed: 0.10, green: 0.18, blue: 0.40, alpha: 1),
])!.draw(in: shape, angle: -90)

NSGraphicsContext.current?.saveGraphicsState()
shape.addClip()

// Background flakes.
var rng = SystemRandomNumberGenerator()
for _ in 0..<70 {
    let r = CGFloat.random(in: 4...13, using: &rng)
    let x = CGFloat.random(in: body.minX...body.maxX, using: &rng)
    let y = CGFloat.random(in: (body.minY + 150)...body.maxY, using: &rng)
    NSColor(white: 1, alpha: .random(in: 0.35...0.85, using: &rng)).setFill()
    NSBezierPath(ovalIn: NSRect(x: x - r, y: y - r, width: r * 2, height: r * 2)).fill()
}

// Snow drift along the bottom.
let drift = NSBezierPath()
drift.move(to: NSPoint(x: body.minX, y: body.minY))
drift.line(to: NSPoint(x: body.minX, y: body.minY + 170))
drift.curve(to: NSPoint(x: body.midX, y: body.minY + 150),
            controlPoint1: NSPoint(x: body.minX + 180, y: body.minY + 240),
            controlPoint2: NSPoint(x: body.midX - 160, y: body.minY + 120))
drift.curve(to: NSPoint(x: body.maxX, y: body.minY + 200),
            controlPoint1: NSPoint(x: body.midX + 170, y: body.minY + 185),
            controlPoint2: NSPoint(x: body.maxX - 160, y: body.minY + 250))
drift.line(to: NSPoint(x: body.maxX, y: body.minY))
drift.close()
NSColor(white: 0.97, alpha: 1).setFill()
drift.fill()
NSGraphicsContext.current?.restoreGraphicsState()

// Big snowflake symbol.
let config = NSImage.SymbolConfiguration(pointSize: 430, weight: .medium)
if let symbol = NSImage(systemSymbolName: "snowflake", accessibilityDescription: nil)?
    .withSymbolConfiguration(config) {
    let tinted = NSImage(size: symbol.size, flipped: false) { rect in
        symbol.draw(in: rect)
        NSColor.white.set()
        rect.fill(using: .sourceAtop)
        return true
    }
    let s = symbol.size
    let target = NSRect(x: canvas.midX - s.width / 2, y: canvas.midY - s.height / 2 + 50,
                        width: s.width, height: s.height)
    let shadow = NSShadow()
    shadow.shadowColor = NSColor(white: 0, alpha: 0.35)
    shadow.shadowBlurRadius = 24
    shadow.shadowOffset = NSSize(width: 0, height: -10)
    shadow.set()
    tinted.draw(in: target)
}

NSGraphicsContext.restoreGraphicsState()

guard let png = rep.representation(using: .png, properties: [:]) else { fatalError("png") }
try png.write(to: URL(fileURLWithPath: CommandLine.arguments[1]))
