# StreamDeckPluginSharp

Cross-platform .NET 8 SDK for [Elgato Stream Deck](https://docs.elgato.com/streamdeck/sdk/) **WebSocket plugins**.

This repository is not a USB HID driver. Stream Deck Module / direct hardware protocols are out of scope. The SDK talks to the Stream Deck application over the official plugin WebSocket.

## Why this SDK

[streamdeck-tools](https://github.com/BarRaider/streamdeck-tools) 7.0 already covers Windows + macOS drawing via SkiaSharp. StreamDeckPluginSharp focuses on the remaining gaps:

- Full Stream Deck plugin protocol v2.0 coverage (`didReceiveDeepLink`, `deviceDidChange`, secrets, resources, `setTriggerDescription`, Stream Deck + dials, and more)
- Async-first handlers that return `Task` and accept `CancellationToken`
- System.Text.Json, no Newtonsoft / NLog / CommandLineParser dependency
- Shared stateful services through `Microsoft.Extensions.DependencyInjection`
- Property Inspector bindings so React forms stay in sync with C# settings without handwritten WebSocket glue
- Windows + macOS packaging from the first sample (`CodePath` / `CodePathMac`, self-contained publish)

## Quick start

```csharp
await StreamDeckPlugin.RunAsync(args);

[StreamDeckAction("dev.example.plugin.counter")]
public sealed class CounterAction : KeyActionBase<CounterSettings>
{
    public override async Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        await UpdateSettingsAsync(settings => settings.Count++, cancellationToken);
        await SetTitleAsync(Settings.Count.ToString(), cancellationToken: cancellationToken);
    }
}
```

Share an external connection across every key:

```csharp
var builder = StreamDeckPlugin.CreateBuilder(args);
builder.Services.AddSingleton<ObsConnectionService>();
await using var plugin = builder.Build();
await plugin.RunAsync();
```

```csharp
public sealed class MuteAction(ObsConnectionService obs) : KeyActionBase<MuteSettings>
{
    public override Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken)
        => obs.ToggleMuteAsync(cancellationToken);
}
```

Implement `IPluginService` on that singleton to start and stop the remote socket with the plugin process. Inject `IStreamDeckConnection` when background code needs to update titles or images.

## Property Inspector

Mark contracts with `[TypeScriptContract]`, generate TypeScript, and bind inputs:

```tsx
const { settings, bind } = useSettings<CounterSettings>();
return <input type="number" {...bind("increment")} />;
```

`StreamDeckProvider` owns `connectElgatoStreamDeckSocket`. See [docs/property-inspector.md](docs/property-inspector.md).

## Sample

[`samples/CounterSample`](samples/CounterSample) shows:

- a keypad action and a Stream Deck + dial sharing one `CounterStore`
- typed settings synchronized with a React inspector
- `publish.ps1` / `publish.sh` for `win-x64`, `osx-arm64`, and `osx-x64`

## Packages

| Package | Purpose |
| --- | --- |
| `StreamDeckPluginSharp` | Plugin host, actions, protocol |
| `StreamDeckPluginSharp.TypeGen` (`sdps-typegen`) | C# → TypeScript contracts |
| `@streamdeckpluginsharp/pi-client` | React hooks for inspectors |

## Documentation

- [Getting started](docs/getting-started.md)
- [Property Inspector](docs/property-inspector.md)

## Build

```bash
dotnet test StreamDeckPluginSharp.sln
```

## License

Apache License 2.0
