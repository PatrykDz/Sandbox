# Snowfall ❄️ (macOS)

A tiny menu-bar app that lets it snow over your Mac desktop.

- Snow falls over everything in a transparent, **click-through** overlay — you keep working normally.
- Flakes **settle on the tops of windows** and on the **Dock**, building small drifts.
- **Drag a window and the snow slides off it**, flung in the direction you moved it.
- Snow on a window melts when the window closes, minimizes or gets covered by another window.
- Depth: distant flakes are smaller, fainter and slower; only near flakes settle.
- Menu bar controls: on/off, intensity (Light / Moderate / Blizzard), wind (Calm / Breeze / Gusty),
  pile-up toggle, "Shake Off Settled Snow", Launch at Login.
- Rendered on the GPU with SpriteKit. Density is halved and frame rate drops to 30 fps on battery
  or in Low Power Mode. Pauses while a full-screen app owns the display and while displays sleep.
- Works on every connected display; rebuilds when displays change.

No permissions are needed: window positions come from `CGWindowListCopyWindowInfo`, which doesn't
require Screen Recording access for bounds.

## Requirements

macOS 14 Sonoma or later, Apple silicon or Intel, Xcode 15+ (or the Swift 5.9+ command line tools).

## Build & run

```bash
cd MacOS/Snowfall
./build-app.sh                 # -> build/Snowfall.app
open build/Snowfall.app
```

`UNIVERSAL=1 ./build-app.sh` produces an arm64 + x86_64 binary (needs full Xcode).
For quick iteration `swift run` also works (Launch at Login needs the `.app` bundle).

Look for the ❄︎ icon in the menu bar. Quit from its menu.

## How it works

| File | Role |
| --- | --- |
| `SnowfallApp.swift` | Entry point; accessory (menu-bar only) app. |
| `AppDelegate.swift` | Status item & menu, one overlay per screen, sleep/screen-change handling. |
| `SnowOverlay.swift` | Borderless, transparent, mouse-ignoring window just above the Dock level, hosting an `SKView`. |
| `SnowScene.swift` | Flake simulation: spawning, drift/wobble/wind, landing on exposed window tops, drift piles, flinging, melting. |
| `WindowTracker.swift` | Polls on-screen window frames (15 Hz), front-to-back, and reports moved/vanished windows with velocity. |
| `PowerMonitor.swift` | Battery / Low Power Mode detection. |
| `Settings.swift` | Persisted preferences (`UserDefaults`). |

Physics runs in global CoreGraphics coordinates (top-left origin, y down) so flakes line up
exactly with window bounds; each display's scene converts to its own SpriteKit coordinates.
