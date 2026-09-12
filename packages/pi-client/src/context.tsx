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
      <div style={{ padding: 12, fontFamily: "Segoe UI, sans-serif" }}>
        Waiting for Stream Deck Property Inspector connection…
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
