import React, { createContext, useContext, useEffect, useMemo, useState } from "react";
import { StreamDeckPiClient, installConnectHook } from "./client";
import type { StreamDeckPiApi } from "./types";

const StreamDeckContext = createContext<StreamDeckPiApi | null>(null);

export function StreamDeckProvider({ children }: { children: React.ReactNode }) {
  const client = useMemo(() => new StreamDeckPiClient(), []);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    installConnectHook(client);
    return client.subscribe("ready", () => setReady(true));
  }, [client]);

  if (!ready) {
    return null;
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
