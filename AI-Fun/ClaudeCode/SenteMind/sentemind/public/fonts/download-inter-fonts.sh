#!/bin/bash

# Download Inter font files from GitHub
# Weights: 300 (Light), 400 (Regular), 500 (Medium), 600 (SemiBold), 700 (Bold)

echo "📦 Downloading Inter font files..."

# Create directory structure
mkdir -p inter

# Base URL for Inter fonts from GitHub
BASE_URL="https://github.com/rsms/inter/raw/master/docs/font-files"

# Download WOFF2 files (primary format for modern browsers)
echo "📥 Downloading WOFF2 files..."
curl -L -o inter/Inter-Light.woff2 "$BASE_URL/Inter-Light.woff2"
curl -L -o inter/Inter-Regular.woff2 "$BASE_URL/Inter-Regular.woff2"
curl -L -o inter/Inter-Medium.woff2 "$BASE_URL/Inter-Medium.woff2"
curl -L -o inter/Inter-SemiBold.woff2 "$BASE_URL/Inter-SemiBold.woff2"
curl -L -o inter/Inter-Bold.woff2 "$BASE_URL/Inter-Bold.woff2"

# Download WOFF files (fallback for older browsers)
echo "📥 Downloading WOFF files..."
curl -L -o inter/Inter-Light.woff "$BASE_URL/Inter-Light.woff"
curl -L -o inter/Inter-Regular.woff "$BASE_URL/Inter-Regular.woff"
curl -L -o inter/Inter-Medium.woff "$BASE_URL/Inter-Medium.woff"
curl -L -o inter/Inter-SemiBold.woff "$BASE_URL/Inter-SemiBold.woff"
curl -L -o inter/Inter-Bold.woff "$BASE_URL/Inter-Bold.woff"

echo "✅ Inter font files downloaded successfully!"
echo "📊 Font files saved to: $(pwd)/inter/"

# List downloaded files
echo "📋 Downloaded files:"
ls -la inter/

echo ""
echo "🚀 Next steps:"
echo "1. Add the fonts.css file to your project"
echo "2. Import fonts.css in your main CSS file"
echo "3. Use font-family: 'Inter', sans-serif in your CSS"