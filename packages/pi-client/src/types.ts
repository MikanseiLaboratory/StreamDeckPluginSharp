export type JsonValue = string | number | boolean | null | JsonValue[] | { [key: string]: JsonValue };

export interface ActionInfo {
  action: string;
  context: string;
  device: string;
  payload?: {
    settings?: Record<string, unknown>;
    coordinates?: { column: number; row: number };
  };
}

export interface StreamDeckPiApi {
  ready: boolean;
  uuid: string;
  actionInfo: ActionInfo | null;
  setSettings(settings: Record<string, unknown>): void;
  getSettings(): void;
  setGlobalSettings(settings: Record<string, unknown>): void;
  getGlobalSettings(): void;
  sendToPlugin(payload: unknown): void;
  subscribe(event: string, handler: (payload: unknown) => void): () => void;
}
