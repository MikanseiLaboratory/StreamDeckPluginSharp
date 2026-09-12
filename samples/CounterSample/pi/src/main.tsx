import React from "react";
import ReactDOM from "react-dom/client";
import { StreamDeckProvider } from "@streamdeckpluginsharp/pi-client";
import { App } from "./App";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <StreamDeckProvider>
      <App />
    </StreamDeckProvider>
  </React.StrictMode>
);
