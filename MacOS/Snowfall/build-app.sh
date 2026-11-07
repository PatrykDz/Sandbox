#!/usr/bin/env bash
# Builds Snowfall.app (menu-bar only, ad-hoc signed) into ./build.
# Usage: ./build-app.sh            # native architecture
#        UNIVERSAL=1 ./build-app.sh  # arm64 + x86_64 (needs full Xcode)
set -euo pipefail
cd "$(dirname "$0")"

if [[ "${UNIVERSAL:-0}" == "1" ]]; then
  swift build -c release --arch arm64 --arch x86_64
  BIN_DIR=".build/apple/Products/Release"
else
  swift build -c release
  BIN_DIR="$(swift build -c release --show-bin-path)"
fi

APP="build/Snowfall.app"
rm -rf "$APP"
mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp "$BIN_DIR/Snowfall" "$APP/Contents/MacOS/Snowfall"

# App icon, rendered from Tools/make-icon.swift.
ICONSET="$(mktemp -d)/Snowfall.iconset"
mkdir -p "$ICONSET"
swift Tools/make-icon.swift "$ICONSET/icon_1024.png"
for size in 16 32 128 256 512; do
  sips -z $size $size "$ICONSET/icon_1024.png" --out "$ICONSET/icon_${size}x${size}.png" >/dev/null
  sips -z $((size * 2)) $((size * 2)) "$ICONSET/icon_1024.png" --out "$ICONSET/icon_${size}x${size}@2x.png" >/dev/null
done
rm "$ICONSET/icon_1024.png"
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/Snowfall.icns"

cat > "$APP/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>Snowfall</string>
  <key>CFBundleDisplayName</key><string>Snowfall</string>
  <key>CFBundleIdentifier</key><string>dev.sandbox.snowfall</string>
  <key>CFBundleExecutable</key><string>Snowfall</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>CFBundleIconFile</key><string>Snowfall</string>
  <key>CFBundleShortVersionString</key><string>1.1</string>
  <key>CFBundleVersion</key><string>2</string>
  <key>LSMinimumSystemVersion</key><string>14.0</string>
  <key>LSUIElement</key><true/>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST

codesign --force --sign - "$APP"
echo "Built $APP"
