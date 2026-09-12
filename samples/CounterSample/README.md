# Counter Sample

Demonstrates StreamDeckPluginSharp:

- `CounterAction` (key) and `DialAction` (Stream Deck + encoder) share one `CounterStore`
- React Property Inspector binds `label` / `increment` to C# `Settings` (no handwritten WebSocket)
- PI buttons send typed `sendToPlugin` commands (`add`, `reset`, `refresh`); the plugin pushes `countChanged` back
- changing Label or Increment in the inspector updates the key title immediately
- `publish.ps1` / `publish.sh` emit self-contained Windows and macOS binaries

## Run

Build, pack a `.streamDeckPlugin`, and install into Stream Deck:

```powershell
./publish.ps1 -Install -Pack
```

Windows-only local install (faster):

```powershell
./publish.ps1 -Runtime win-x64 -Install
```

Repository-wide pack (NuGet + sample plugin) from the repo root:

```powershell
./pack.ps1
```

Then add **Shared Counter** to a key and open its Property Inspector:

- Edit **Label** / **Increment** — the key title updates from C# via `didReceiveSettings`
- **Add** / **Reset** — typed `sendToPlugin` commands change the shared count; the inspector receives `countChanged`
- Press the key or rotate the dial — the same count is reflected in the inspector

Install destination:
- Windows: `%APPDATA%\Elgato\StreamDeck\Plugins\dev.flowingspdg.countersample.sdPlugin`
- macOS: `~/Library/Application Support/com.elgato.StreamDeck/Plugins/dev.flowingspdg.countersample.sdPlugin`
