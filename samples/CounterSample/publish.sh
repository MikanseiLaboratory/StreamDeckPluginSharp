#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
CONFIG="${1:-Release}"
PLUGIN_DIR="$ROOT/com.flowingspdg.countersample.sdPlugin"

dotnet build "$ROOT/../../src/StreamDeckPluginSharp.TypeGen/StreamDeckPluginSharp.TypeGen.csproj" -c "$CONFIG"
dotnet build "$ROOT/CounterSample.csproj" -c "$CONFIG"
TYPEGEN="$ROOT/../../src/StreamDeckPluginSharp.TypeGen/bin/$CONFIG/net8.0/StreamDeckPluginSharp.TypeGen.dll"
ASSEMBLY="$ROOT/bin/$CONFIG/net8.0/dev.flowingspdg.countersample.dll"
dotnet exec "$TYPEGEN" "$ASSEMBLY" "$ROOT/pi/src/generated/contracts.ts"

(cd "$ROOT/pi" && { [ -d node_modules ] || npm install; } && npm run build)

for rid in win-x64 osx-arm64 osx-x64; do
  dotnet publish "$ROOT/CounterSample.csproj" -c "$CONFIG" -r "$rid" --self-contained true -o "$PLUGIN_DIR/bin/$rid"
done

echo "Published plugin bundle to $PLUGIN_DIR"
