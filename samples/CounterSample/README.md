# Counter Sample

Demonstrates StreamDeckPluginSharp:

- `CounterAction` (key) and `DialAction` (Stream Deck + encoder) share one `CounterStore`
- settings (`increment`) bind automatically to a React Property Inspector
- `publish.ps1` / `publish.sh` emit self-contained Windows and macOS binaries

## Run

1. Build and publish:

```powershell
./publish.ps1
```

2. Copy `com.flowingspdg.countersample.sdPlugin` into the Stream Deck plugins folder, or use [Elgato CLI](https://docs.elgato.com/streamdeck/sdk/guides/packaging) `streamdeck restart` / link.
3. Add **Shared Counter** to a key. Change increment in the inspector; press the key. The dial action updates the same count.

Windows plugins folder: `%APPDATA%\Elgato\StreamDeck\Plugins`  
macOS plugins folder: `~/Library/Application Support/com.elgato.StreamDeck/Plugins`
