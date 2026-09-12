# @streamdeckpluginsharp/pi-client

React helpers for Stream Deck Property Inspectors. The provider owns the WebSocket; your UI binds typed settings.

This package is intended for npm (`@streamdeckpluginsharp/pi-client`). This repository currently consumes it via a `file:` path so the sample stays in lockstep with the SDK. Publish it when the first NuGet SDK release goes out, so plugin authors can `npm install` the matching PI client.

```tsx
import { StreamDeckProvider, useSettings } from "@streamdeckpluginsharp/pi-client";

function Panel() {
  const { bind } = useSettings<{ increment: number }>({ increment: 1 });
  return <input type="number" {...bind("increment")} />;
}
```
