import { useCallback, useEffect, useState } from "react";
import { useStreamDeck } from "./context";

type SettingsRecord = Record<string, unknown>;

export function useSettings<T extends SettingsRecord>(defaults?: Partial<T>) {
  const client = useStreamDeck();
  const [settings, setLocal] = useState<T>(() => ({
    ...(defaults ?? {}),
    ...(client.actionInfo?.payload?.settings ?? {})
  } as T));

  useEffect(() => {
    return client.subscribe("didReceiveSettings", (payload) => {
      setLocal((previous) => ({ ...previous, ...(payload as T) }));
    });
  }, [client]);

  const setSettings = useCallback(
    (next: T | ((previous: T) => T)) => {
      setLocal((previous) => {
        const value = typeof next === "function" ? (next as (previous: T) => T)(previous) : next;
        client.setSettings(value);
        return value;
      });
    },
    [client]
  );

  const bind = useCallback(
    <K extends keyof T>(name: K) => ({
      name: String(name),
      value: settings[name] as T[K],
      onChange: (event: { target: { type: string; value: string; checked: boolean } }) => {
        const target = event.target;
        const value =
          target.type === "checkbox"
            ? target.checked
            : target.type === "number"
              ? Number(target.value)
              : target.value;
        setSettings((previous) => ({ ...previous, [name]: value }));
      }
    }),
    [settings, setSettings]
  );

  return { settings, setSettings, bind };
}

export function useGlobalSettings<T extends SettingsRecord>(defaults?: Partial<T>) {
  const client = useStreamDeck();
  const [settings, setLocal] = useState<T>(() => ({ ...(defaults ?? {}) } as T));

  useEffect(() => {
    client.getGlobalSettings();
    return client.subscribe("didReceiveGlobalSettings", (payload) => {
      setLocal((previous) => ({ ...previous, ...(payload as T) }));
    });
  }, [client]);

  const setSettings = useCallback(
    (next: T | ((previous: T) => T)) => {
      setLocal((previous) => {
        const value = typeof next === "function" ? (next as (previous: T) => T)(previous) : next;
        client.setGlobalSettings(value);
        return value;
      });
    },
    [client]
  );

  return { settings, setSettings };
}

export function sendToPlugin<T>(payload: T): void {
  throw new Error("sendToPlugin(payload) requires the hook form. Use useSendToPlugin().");
}

export function useSendToPlugin() {
  const client = useStreamDeck();
  return useCallback(<T,>(payload: T) => client.sendToPlugin(payload), [client]);
}

export function usePluginMessage<T>(handler: (payload: T) => void): void {
  const client = useStreamDeck();
  useEffect(() => client.subscribe("sendToPropertyInspector", (payload) => handler(payload as T)), [client, handler]);
}
