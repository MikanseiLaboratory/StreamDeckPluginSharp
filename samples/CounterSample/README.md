# Counter Sample

Demonstrates StreamDeckPluginSharp:

- `CounterAction` (key) and `DialAction` (Stream Deck + encoder) share one `CounterStore`
- settings (`increment`) bind automatically to a React Property Inspector
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

Then add **Shared Counter** to a key. Change increment in the inspector; press the key. The dial action updates the same count.

Install destination:
- Windows: `%APPDATA%\Elgato\StreamDeck\Plugins\dev.flowingspdg.countersample.sdPlugin`
- macOS: `~/Library/Application Support/com.elgato.StreamDeck/Plugins/dev.flowingspdg.countersample.sdPlugin`
