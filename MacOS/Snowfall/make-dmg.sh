#!/usr/bin/env bash
# Packages build/Snowfall.app into build/Snowfall.dmg (drag-to-Applications installer).
# Run ./build-app.sh first.
set -euo pipefail
cd "$(dirname "$0")"

APP="build/Snowfall.app"
DMG="build/Snowfall.dmg"
[[ -d "$APP" ]] || { echo "Missing $APP — run ./build-app.sh first" >&2; exit 1; }

STAGE="$(mktemp -d)"
trap 'rm -rf "$STAGE"' EXIT
cp -R "$APP" "$STAGE/"
ln -s /Applications "$STAGE/Applications"
cat > "$STAGE/READ ME FIRST.txt" <<'TXT'
Install: drag Snowfall into the Applications folder, then open it from Applications.

Snowfall isn't notarized by Apple, so the first launch is blocked:
  1. Double-click Snowfall and dismiss the warning.
  2. Open System Settings → Privacy & Security, scroll down and click "Open Anyway"
     next to the Snowfall message, then confirm.
(Or in Terminal: xattr -dr com.apple.quarantine /Applications/Snowfall.app)

Snowfall lives in the menu bar (❄︎ icon) — there is no Dock icon or window.
Use its menu for intensity, wind, Launch at Login and Quit.
TXT

rm -f "$DMG"
hdiutil create -volname "Snowfall" -srcfolder "$STAGE" -ov -format UDZO "$DMG"
echo "Built $DMG"
