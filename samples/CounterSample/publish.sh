#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$ROOT/../.." && pwd)"
CONFIG="${CONFIG:-Release}"
PLUGIN_ID="dev.flowingspdg.countersample"
PLUGIN_DIR="$ROOT/$PLUGIN_ID.sdPlugin"
OUTPUT_DIR="${OUTPUT_DIR:-$REPO_ROOT/artifacts/plugin}"
RUNTIMES="${RUNTIMES:-win-x64 osx-arm64 osx-x64}"
INSTALL="${INSTALL:-0}"
PACK="${PACK:-0}"

dotnet build "$ROOT/../../src/StreamDeckPluginSharp.TypeGen/StreamDeckPluginSharp.TypeGen.csproj" -c "$CONFIG"
dotnet build "$ROOT/CounterSample.csproj" -c "$CONFIG"
TYPEGEN="$ROOT/../../src/StreamDeckPluginSharp.TypeGen/bin/$CONFIG/net8.0/StreamDeckPluginSharp.TypeGen.dll"
ASSEMBLY="$ROOT/bin/$CONFIG/net8.0/dev.flowingspdg.countersample.dll"
dotnet exec "$TYPEGEN" "$ASSEMBLY" "$ROOT/pi/src/generated/contracts.ts"

(cd "$ROOT/pi" && { [ -d node_modules ] || npm install; } && npm run build)

for rid in $RUNTIMES; do
  rm -rf "$PLUGIN_DIR/bin/$rid"
  dotnet publish "$ROOT/CounterSample.csproj" -c "$CONFIG" -r "$rid" --self-contained true -o "$PLUGIN_DIR/bin/$rid"
  find "$PLUGIN_DIR/bin/$rid" -name '*.pdb' -delete
done

echo "Published plugin bundle to $PLUGIN_DIR"

if [ "$PACK" = "1" ]; then
  mkdir -p "$OUTPUT_DIR"
  ZIP="$OUTPUT_DIR/$PLUGIN_ID.streamDeckPlugin"
  rm -f "$ZIP"
  if command -v streamdeck >/dev/null 2>&1; then
    streamdeck pack "$PLUGIN_DIR" --output "$OUTPUT_DIR" --force
  else
    (cd "$(dirname "$PLUGIN_DIR")" && zip -qr "$ZIP" "$(basename "$PLUGIN_DIR")")
  fi
  echo "Packed $ZIP"
fi

if [ "$INSTALL" = "1" ]; then
  DEST="$HOME/Library/Application Support/com.elgato.StreamDeck/Plugins/$PLUGIN_ID.sdPlugin"
  mkdir -p "$(dirname "$DEST")"
  rm -rf "$DEST"
  cp -R "$PLUGIN_DIR" "$DEST"
  echo "Installed plugin to $DEST"
  if command -v streamdeck >/dev/null 2>&1; then
    streamdeck restart "$PLUGIN_ID"
  fi
fi
