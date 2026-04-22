#!/usr/bin/env bash
# Release the Logi Options+ companion plugin as a GitHub release with a .lplug4 asset.
#
# Usage:   ./scripts/release-plugin.sh <version>
# Example: ./scripts/release-plugin.sh 0.1.0
#
# Must run locally — PluginApi.dll comes from a Logi Options+ install and
# is not available in CI. Requires: dotnet, logiplugintool, gh, git.

set -euo pipefail

if [ $# -lt 1 ]; then
  echo "usage: $0 <version>   (e.g. 0.1.0)"
  exit 1
fi

VERSION="$1"
TAG="plugin-v${VERSION}"
REPO_ROOT="$(cd "$(dirname "$0")/.." && pwd)"
MANIFEST="$REPO_ROOT/logi-plugin/src/package/metadata/LoupedeckPackage.yaml"
OUT_DIR="$REPO_ROOT/logi-plugin/build"
OUT_FILE="$OUT_DIR/HapticBridgeForUnity_${VERSION}.lplug4"

echo ">> checking preconditions"
command -v dotnet >/dev/null || { echo "dotnet not found"; exit 1; }
command -v gh >/dev/null || { echo "gh not found"; exit 1; }
command -v logiplugintool >/dev/null || { echo "logiplugintool not found — run: dotnet tool install --global LogiPluginTool"; exit 1; }

if [ -n "$(git -C "$REPO_ROOT" status --porcelain)" ]; then
  echo "working tree not clean — commit or stash first"
  git -C "$REPO_ROOT" status --short
  exit 1
fi

if git -C "$REPO_ROOT" rev-parse "$TAG" >/dev/null 2>&1; then
  echo "tag $TAG already exists"
  exit 1
fi

echo ">> syncing version=$VERSION in manifest"
if ! grep -q "^version: $VERSION$" "$MANIFEST"; then
  # BSD sed (macOS) compatible in-place edit
  sed -i '' -E "s/^version: .*/version: $VERSION/" "$MANIFEST"
  git -C "$REPO_ROOT" add "$MANIFEST"
  git -C "$REPO_ROOT" commit -m "Bump plugin version to $VERSION"
  git -C "$REPO_ROOT" push
fi

echo ">> building plugin (Release)"
(cd "$REPO_ROOT/logi-plugin/src" && dotnet build -c Release)

echo ">> packing .lplug4"
mkdir -p "$OUT_DIR"
rm -f "$OUT_FILE"
logiplugintool pack "$REPO_ROOT/logi-plugin/bin/Release" "$OUT_FILE"
ls -la "$OUT_FILE"

echo ">> tagging $TAG"
git -C "$REPO_ROOT" tag -a "$TAG" -m "Plugin $VERSION"
git -C "$REPO_ROOT" push origin "$TAG"

echo ">> creating GitHub release"
gh release create "$TAG" "$OUT_FILE" \
  --title "HapticBridge for Unity — Plugin v$VERSION" \
  --generate-notes \
  --notes "Double-click the .lplug4 asset to install in Logi Options+. See repo root README for details."

echo
echo "done — https://github.com/edinylcnn/LogiHapticsForUnity/releases/tag/$TAG"
