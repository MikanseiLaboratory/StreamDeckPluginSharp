import { usePluginMessage, useSendToPlugin, useSettings } from "@streamdeckpluginsharp/pi-client";
import { useState } from "react";
import type { CountChangedMessage, CounterSettings, PropertyInspectorCommand } from "./generated/contracts";

export function App() {
  const { settings, bind } = useSettings<CounterSettings>({ label: "Count", increment: 1 });
  const [count, setCount] = useState(0);
  const sendToPlugin = useSendToPlugin();

  usePluginMessage<CountChangedMessage>((message) => {
    if (message.type === "countChanged") {
      setCount(message.count);
    }
  });

  const command = (type: PropertyInspectorCommand["type"]) => {
    sendToPlugin<PropertyInspectorCommand>({ type });
  };

  return (
    <div className="sdpi-wrapper" style={{ fontFamily: "Segoe UI, sans-serif", padding: 12, display: "grid", gap: 10 }}>
      <div>
        <label htmlFor="label">Label</label>
        <input id="label" type="text" {...bind("label")} />
      </div>
      <div>
        <label htmlFor="increment">Increment</label>
        <input id="increment" type="number" {...bind("increment")} />
      </div>
      <div>
        Shared count: <strong>{count}</strong>
      </div>
      <div style={{ display: "flex", gap: 8 }}>
        <button type="button" onClick={() => command("add")}>
          Add {settings.increment || 1}
        </button>
        <button type="button" onClick={() => command("reset")}>
          Reset
        </button>
        <button type="button" onClick={() => command("refresh")}>
          Refresh
        </button>
      </div>
      <p style={{ margin: 0, opacity: 0.75 }}>
        Label and increment are saved with <code>setSettings</code> and appear on the key title from C#.
        Add / Reset talk to the plugin over typed <code>sendToPlugin</code> messages.
      </p>
    </div>
  );
}
