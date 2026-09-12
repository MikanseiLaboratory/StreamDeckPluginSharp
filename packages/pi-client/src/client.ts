import type { ActionInfo, StreamDeckPiApi } from "./types";

interface Incoming {
  event?: string;
  payload?: {
    settings?: Record<string, unknown>;
  } & Record<string, unknown>;
}

export class StreamDeckPiClient implements StreamDeckPiApi {
  private socket: WebSocket | null = null;
  private listeners = new Map<string, Set<(payload: unknown) => void>>();
  private pending: string[] = [];
  private debounceTimer: ReturnType<typeof setTimeout> | null = null;
  private latestSettings: Record<string, unknown> = {};

  ready = false;
  uuid = "";
  actionInfo: ActionInfo | null = null;

  connect(port: string, uuid: string, registerEvent: string, _info: string, actionInfo: string): void {
    this.uuid = uuid;
    this.actionInfo = JSON.parse(actionInfo) as ActionInfo;
    this.latestSettings = this.actionInfo.payload?.settings ?? {};
    this.socket = new WebSocket(`ws://127.0.0.1:${port}`);
    this.socket.onopen = () => {
      this.socket?.send(JSON.stringify({ event: registerEvent, uuid }));
      this.ready = true;
      for (const queued of this.pending) {
        this.socket?.send(queued);
      }
      this.pending = [];
      this.emit("ready", this.actionInfo);
      this.emit("didReceiveSettings", this.latestSettings);
      this.getSettings();
    };
    this.socket.onmessage = (event) => {
      const message = JSON.parse(String(event.data)) as Incoming;
      if (!message.event) {
        return;
      }
      if (message.event === "didReceiveSettings") {
        this.latestSettings = message.payload?.settings ?? {};
        this.emit("didReceiveSettings", this.latestSettings);
        return;
      }
      if (message.event === "didReceiveGlobalSettings") {
        this.emit("didReceiveGlobalSettings", message.payload?.settings ?? {});
        return;
      }
      if (message.event === "sendToPropertyInspector") {
        this.emit("sendToPropertyInspector", message.payload);
        return;
      }
      this.emit(message.event, message.payload);
    };
  }

  setSettings(settings: Record<string, unknown>): void {
    this.latestSettings = settings;
    if (this.debounceTimer) {
      clearTimeout(this.debounceTimer);
    }
    this.debounceTimer = setTimeout(() => {
      this.send({ event: "setSettings", context: this.uuid, payload: settings });
    }, 120);
  }

  getSettings(): void {
    this.send({ event: "getSettings", context: this.uuid });
  }

  setGlobalSettings(settings: Record<string, unknown>): void {
    this.send({ event: "setGlobalSettings", context: this.uuid, payload: settings });
  }

  getGlobalSettings(): void {
    this.send({ event: "getGlobalSettings", context: this.uuid });
  }

  sendToPlugin(payload: unknown): void {
    this.send({
      event: "sendToPlugin",
      action: this.actionInfo?.action,
      context: this.uuid,
      payload
    });
  }

  subscribe(event: string, handler: (payload: unknown) => void): () => void {
    const set = this.listeners.get(event) ?? new Set();
    set.add(handler);
    this.listeners.set(event, set);
    return () => set.delete(handler);
  }

  private emit(event: string, payload: unknown): void {
    this.listeners.get(event)?.forEach((handler) => handler(payload));
  }

  private send(message: object): void {
    const json = JSON.stringify(message);
    if (this.socket?.readyState === WebSocket.OPEN) {
      this.socket.send(json);
      return;
    }
    this.pending.push(json);
  }
}

export function installConnectHook(client: StreamDeckPiClient): void {
  (window as Window & { connectElgatoStreamDeckSocket?: typeof client.connect }).connectElgatoStreamDeckSocket = (
    port,
    uuid,
    registerEvent,
    info,
    actionInfo
  ) => client.connect(port, uuid, registerEvent, info, actionInfo);
}
