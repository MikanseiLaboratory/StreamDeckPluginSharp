# Property Inspector bindings

The goal is that plugin authors never write WebSocket glue. Settings are a typed C# class and a matching React form.

## C# contract

```csharp
[TypeScriptContract]
public sealed class CounterSettings
{
    public int Increment { get; set; } = 1;
}
```

The action base class deserializes `didReceiveSettings` into `Settings` and calls `OnSettingsChangedAsync(previous, current, ct)`. To push a change back to the inspector:

```csharp
await UpdateSettingsAsync(settings => settings.Increment = 2, cancellationToken);
```

Custom messages still use `SendToPropertyInspectorAsync<T>` and `OnPropertyInspectorMessageAsync`.

## Generate TypeScript

```bash
dotnet tool install -g StreamDeckPluginSharp.TypeGen
sdps-typegen ./bin/Release/net8.0/MyPlugin.dll ./pi/src/generated/contracts.ts
```

The generator emits camelCase interfaces that match System.Text.Json's default naming.

## React inspector

```tsx
import { StreamDeckProvider, useSettings } from "@streamdeckpluginsharp/pi-client";
import type { CounterSettings } from "./generated/contracts";

export function App() {
  const { settings, bind } = useSettings<CounterSettings>({ increment: 1 });
  return (
    <div>
      <input type="number" {...bind("increment")} />
      <span>Step: {settings.increment}</span>
    </div>
  );
}

// index
<StreamDeckProvider>
  <App />
</StreamDeckProvider>
```

`StreamDeckProvider` owns `connectElgatoStreamDeckSocket`, the WebSocket, registration, and `didReceiveSettings` subscriptions. `bind("field")` writes through a short debounce so typing does not flood Stream Deck.

Build the inspector with Vite (`base: './'`) and place the output in `*.sdPlugin/ui/`. Point `PropertyInspectorPath` at `ui/index.html`.
