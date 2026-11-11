Snowfall is a small menu-bar app that lets it snow over your Mac desktop. Snow falls in a see-through layer, piles up on window tops and the Dock, and slides off when you drag a window.

**Requirements:** macOS 14 Sonoma or later, Apple silicon or Intel.

### Install
1. Download **Snowfall.dmg** below, open it, and drag **Snowfall** into **Applications**.
2. Open Snowfall. The app isn't notarized by Apple, so the first launch is blocked: close the warning, go to **System Settings → Privacy & Security**, click **Open Anyway** next to Snowfall, and confirm. You only need to do this once.
   Or run this in Terminal: `xattr -dr com.apple.quarantine /Applications/Snowfall.app`
3. Look for the ❄︎ icon in the menu bar. Snowfall has no Dock icon or window.

### New in 1.2
- Settled snow now behaves like a real mass: it rides along when you move a window gently, slides and spills off the edge when you jerk the window, and lifts off and falls when you yank the window down
- Falling snow comes down in clumps under gravity, crumbles and trails powder, and piles up again where it lands
- Drifts form a smooth natural slope and crumble over the ends when they get deep
- Resizing a window keeps the snow in place

### Features
- Snow falls over everything, and clicks pass straight through to what's underneath
- Snow builds smooth drifts on window tops and the Dock
- The mouse pointer swirls falling snow, and a quick swipe knocks drifts loose
- Snow on a window is kept while the window is covered, minimised or on another Space
- Menu settings: intensity (Light, Moderate, Blizzard), wind (Calm, Breeze, Gusty), piling on or off, pointer effects on or off, Shake Off Settled Snow, Launch at Login
- Lighter snow on battery, in Low Power Mode and with Reduce Motion; pauses during full-screen apps and while displays sleep
- No permissions needed
