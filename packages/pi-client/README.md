# @streamdeckpluginsharp/pi-client

React helpers for Stream Deck Property Inspectors. The provider owns the WebSocket; your UI binds typed settings.

```tsx
import { StreamDeckProvider, useSettings } from "@streamdeckpluginsharp/pi-client";

function Panel() {
  const { bind } = useSettings<{ increment: number }>({ increment: 1 });
  return <input type="number" {...bind("increment")} />;
}
```
