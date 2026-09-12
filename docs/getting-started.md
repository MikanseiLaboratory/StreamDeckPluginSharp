# Getting started

StreamDeckPluginSharp is a .NET 8 SDK for **Stream Deck WebSocket plugins**. It does not talk to Stream Deck hardware over USB HID.

## 1. Create a plugin project

```bash
dotnet new console -n MyPlugin -f net8.0
cd MyPlugin
dotnet add package StreamDeckPluginSharp
```

## 2. Define an action

```csharp
using StreamDeckPluginSharp;
using StreamDeckPluginSharp.Actions;
using StreamDeckPluginSharp.Events;

[StreamDeckAction("dev.example.myplugin.counter")]
public sealed class CounterAction : KeyActionBase<CounterSettings>
{
    public override async Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        await UpdateSettingsAsync(settings => settings.Count++, cancellationToken);
        await SetTitleAsync(Settings.Count.ToString(), cancellationToken: cancellationToken);
    }
}

[TypeScriptContract]
public sealed class CounterSettings
{
    public int Count { get; set; }
}
```

`Settings` is kept in sync automatically. When the Property Inspector changes a field, `OnSettingsChangedAsync` runs. When the plugin updates settings, call `UpdateSettingsAsync` so Stream Deck and the inspector both receive the new values.

## 3. Start the host

```csharp
using StreamDeckPluginSharp;

await StreamDeckPlugin.RunAsync(args);
```

Stream Deck launches your executable with `-port`, `-pluginUUID`, `-registerEvent`, and `-info`. The host connects to `ws://127.0.0.1:{port}` and registers itself.

## 4. Share stateful connections

```csharp
var builder = StreamDeckPlugin.CreateBuilder(args);
builder.Services.AddSingleton<ObsConnectionService>();
await using var plugin = builder.Build();
await plugin.RunAsync();
```

Register a singleton that implements `IPluginService` to start and stop an external WebSocket when the plugin starts. Inject that singleton into any action constructor. Inject `IStreamDeckConnection` when a background service needs to update keys.

## 5. Package for Windows and macOS

Your `manifest.json` should include both platforms:

```json
{
  "CodePath": "MyPlugin.exe",
  "CodePathMac": "MyPlugin",
  "OS": [
    { "Platform": "windows", "MinimumVersion": "10" },
    { "Platform": "mac", "MinimumVersion": "12" }
  ],
  "SDKVersion": 2,
  "Software": { "MinimumVersion": "6.5" }
}
```

Publish self-contained binaries for `win-x64`, `osx-arm64`, and `osx-x64`, then copy them into the `.sdPlugin` folder. See `samples/CounterSample/publish.ps1` and `publish.sh`.
