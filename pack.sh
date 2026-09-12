#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
CONFIG="${1:-Release}"
NUGET_OUT="$ROOT/artifacts/nuget"
NPM_OUT="$ROOT/artifacts/npm"
PLUGIN_OUT="$ROOT/artifacts/plugin"

mkdir -p "$NUGET_OUT" "$NPM_OUT"
dotnet pack "$ROOT/src/StreamDeckPluginSharp/StreamDeckPluginSharp.csproj" -c "$CONFIG" -o "$NUGET_OUT"
dotnet pack "$ROOT/src/StreamDeckPluginSharp.TypeGen/StreamDeckPluginSharp.TypeGen.csproj" -c "$CONFIG" -o "$NUGET_OUT"
(cd "$ROOT/packages/pi-client" && npm pack --pack-destination "$NPM_OUT")

CONFIG="$CONFIG" PACK=1 OUTPUT_DIR="$PLUGIN_OUT" "$ROOT/samples/CounterSample/publish.sh"

echo "NuGet packages: $NUGET_OUT"
echo "npm package: $NPM_OUT"
echo "Plugin package: $PLUGIN_OUT"
