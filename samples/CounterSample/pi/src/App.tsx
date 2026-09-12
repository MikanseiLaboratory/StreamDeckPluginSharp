import { usePluginMessage, useSendToPlugin, useSettings } from "@streamdeckpluginsharp/pi-client";
import { useState } from "react";
import type { CountChangedMessage, CounterSettings } from "./generated/contracts";

export function App() {
  const { settings, bind } = useSettings<CounterSettings>({ increment: 1 });
  const [count, setCount] = useState(0);
  const sendToPlugin = useSendToPlugin();

  usePluginMessage<CountChangedMessage>((message) => {
    if (message.type === "countChanged") {
      setCount(message.count);
    }
  });

  return (
    <div className="sdpi-wrapper" style={{ fontFamily: "Segoe UI, sans-serif", padding: 12 }}>
      <div className="sdpi-item">
        <label className="sdpi-item-label" htmlFor="increment">
          Increment
        </label>
        <input id="increment" type="number" {...bind("increment")} />
      </div>
      <div className="sdpi-item">
        <span>Shared count: {count}</span>
      </div>
      <button type="button" onClick={() => sendToPlugin({ type: "refresh" })}>
        Refresh
      </button>
    </div>
  );
}
