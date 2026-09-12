# @streamdeckpluginsharp/pi-client

React helpers for Stream Deck Property Inspectors. The provider owns the WebSocket; your UI binds typed settings.

This package is published to npm as `@streamdeckpluginsharp/pi-client` from the `v*` release workflow (OIDC Trusted Publishing, no npm token). This repository still consumes it via a `file:` path so the sample stays in lockstep with the SDK.

```tsx
import { StreamDeckProvider, useSettings } from "@streamdeckpluginsharp/pi-client";

function Panel() {
  const { bind } = useSettings<{ increment: number }>({ increment: 1 });
  return <input type="number" {...bind("increment")} />;
}
```
