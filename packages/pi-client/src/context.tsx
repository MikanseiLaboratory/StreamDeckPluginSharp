import React, { createContext, useContext, useEffect, useState } from "react";
import { StreamDeckPiClient, installConnectHook } from "./client";
import type { StreamDeckPiApi } from "./types";

const client = new StreamDeckPiClient();
installConnectHook(client);

const StreamDeckContext = createContext<StreamDeckPiApi | null>(null);

export function StreamDeckProvider({ children }: { children: React.ReactNode }) {
  const [ready, setReady] = useState(client.ready);

  useEffect(() => client.subscribe("ready", () => setReady(true)), []);

  if (!ready) {
    return (
      <div className="sdpi-wrapper">
        <div className="sdpi-heading">PROPERTY INSPECTOR</div>
        <div className="sdpi-item">
          <div className="sdpi-item-label">Status</div>
          <div className="sdpi-item-value">Connecting…</div>
        </div>
      </div>
    );
  }

  return <StreamDeckContext.Provider value={client}>{children}</StreamDeckContext.Provider>;
}

export function useStreamDeck(): StreamDeckPiApi {
  const value = useContext(StreamDeckContext);
  if (!value) {
    throw new Error("useStreamDeck must be used inside StreamDeckProvider.");
  }
  return value;
}
