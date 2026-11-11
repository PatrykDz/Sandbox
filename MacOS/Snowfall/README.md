# Snowfall ❄️ (macOS)

A tiny menu-bar app that lets it snow over your Mac desktop.

- Snow falls over everything in a transparent, **click-through** overlay — you keep working normally.
- Flakes **settle on the tops of windows** and on the **Dock** and merge into one smooth drift.
  Steep heaps avalanche into a natural slope, and deep drifts crumble over the ends.
- **The drift is a physical body** with its own inertia, held on by friction and gravity:
  - move a window gently and the snow rides along;
  - jerk it sideways and the snow slides along the top and spills off the trailing edge;
  - stop suddenly and it keeps going and tumbles off the front;
  - yank the window down (or stop after pushing it up) and the drift lifts off, breaks into
    clumps and falls.
- **Falling clumps obey gravity and air drag**: big clumps fall faster than small ones, they
  crumble and trail powder on the way down, and they pile up again on whatever they land on
  (another window, the Dock, or the same window once it stops).
- **Your pointer stirs the snow**: falling flakes swirl in its wake, and a quick swipe ploughs a
  gap through a drift.
- Drifts are **kept, not lost**, when a window is covered by another one, minimized, or on another
  Space; they reappear when the window comes back to the same place. Resizing a window leaves the
  snow where it is and drops whatever no longer has a top edge under it.
- Depth: distant flakes are smaller, fainter and slower; only near flakes settle. A few of the
  closest flakes are slowly spinning six-armed crystals.
- The sky is already full of snow at launch.
- Menu bar controls: on/off, intensity (Light / Moderate / Blizzard), wind (Calm / Breeze / Gusty),
  pile-up toggle, pointer toggle, "Shake Off Settled Snow", Launch at Login.
- Rendered on the GPU with SpriteKit. Density is halved and frame rate drops to 30 fps on battery
  or in Low Power Mode; density is also halved with Reduce Motion. Everything (including window
  polling) pauses while a full-screen app owns the display and while displays sleep.
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
| `SnowScene.swift` | The simulation: falling flakes, per-surface height-map drifts (friction, sliding, lift-off, avalanching, melting), falling clumps and powder, pointer interaction, drawing. |
| `WindowTracker.swift` | Polls on-screen window frames (15 Hz idle, 60 Hz while a window is moving), front-to-back, and reports moved/vanished windows with velocity. |
| `PowerMonitor.swift` | Battery / Low Power Mode detection. |
| `Settings.swift` | Persisted preferences (`UserDefaults`). |

Physics runs in global CoreGraphics coordinates (top-left origin, y down) so flakes line up
exactly with window bounds; each display's scene converts to its own SpriteKit coordinates.
