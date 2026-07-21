import { useCallback, useEffect, useRef, useState } from "react";
import MapView from "./MapView";
import { TMM_WEBSOCKET_URL } from "./constants";
import {
  connectReceiver,
  fetchAccessCodeV2FromBackend,
  fetchReceiverInfo,
  fetchTmmPublicKey,
  submitPublicKeyToBackend,
} from "./tmmApi";
import type { GnssPosition, LocationV2DataMessage } from "./types";
import "./App.css";

type ConnectionState = "disconnected" | "connecting" | "connected";

function parsePosition(message: LocationV2DataMessage): GnssPosition | null {
  if (message.latitude == null || message.longitude == null) {
    return null;
  }

  return {
    latitude: message.latitude,
    longitude: message.longitude,
  };
}

export default function App() {
  const [connectionState, setConnectionState] = useState<ConnectionState>("disconnected");
  const [position, setPosition] = useState<GnssPosition | null>(null);
  const [statusMessage, setStatusMessage] = useState("");
  const [error, setError] = useState("");
  const webSocketRef = useRef<WebSocket | null>(null);
  const disconnectingRef = useRef(false);

  const closeWebSocket = useCallback(() => {
    const socket = webSocketRef.current;
    if (!socket) {
      return;
    }

    disconnectingRef.current = true;
    socket.onopen = null;
    socket.onmessage = null;
    socket.onerror = null;
    socket.onclose = null;

    if (socket.readyState === WebSocket.OPEN || socket.readyState === WebSocket.CONNECTING) {
      socket.close();
    }

    webSocketRef.current = null;
  }, []);

  const disconnect = useCallback(() => {
    closeWebSocket();
    disconnectingRef.current = false;
    setPosition(null);
    setConnectionState("disconnected");
    setStatusMessage("");
    setError("");
  }, [closeWebSocket]);

  const openPositionStream = useCallback(() => {
    return new Promise<void>((resolve, reject) => {
      disconnectingRef.current = false;
      const socket = new WebSocket(TMM_WEBSOCKET_URL);
      webSocketRef.current = socket;

      socket.onopen = () => {
        setConnectionState("connected");
        setStatusMessage("Receiving GNSS positions from TMM.");
        resolve();
      };

      socket.onmessage = (event) => {
        try {
          const message = JSON.parse(String(event.data)) as LocationV2DataMessage;
          const nextPosition = parsePosition(message);
          if (nextPosition) {
            setPosition(nextPosition);
          }
        } catch {
          // Ignore malformed messages.
        }
      };

      socket.onerror = () => {
        reject(new Error("WebSocket connection failed."));
      };

      socket.onclose = () => {
        if (webSocketRef.current === socket) {
          webSocketRef.current = null;
        }

        if (!disconnectingRef.current) {
          setError("Position stream disconnected unexpectedly.");
          setConnectionState("disconnected");
          setPosition(null);
          setStatusMessage("");
        }
      };
    });
  }, []);

  const connect = useCallback(async () => {
    setError("");
    setStatusMessage("");
    setConnectionState("connecting");

    try {
      setStatusMessage("Checking Trimble Mobile Manager...");
      let publicKey;
      try {
        publicKey = await fetchTmmPublicKey();
      } catch {
        throw new Error("Trimble Mobile Manager is not running. Please launch Trimble Mobile Manager.");
      }

      setStatusMessage("Registering public key with backend...");
      await submitPublicKeyToBackend(publicKey);

      setStatusMessage("Generating access code...");
      let accessCodeV2;
      try {
        accessCodeV2 = await fetchAccessCodeV2FromBackend();
      } catch (error) {
        setStatusMessage(error instanceof Error ? error.message : "Failed to generate access code.");
        throw error;
      }

      setStatusMessage("Checking GNSS receiver status...");
      let receiverInfo = await fetchReceiverInfo(accessCodeV2);

      if (!receiverInfo.isReceiverConfigured) {
        throw new Error(
          "No GNSS receiver is configured. Open Trimble Mobile Manager and connect to a GNSS receiver.",
        );
      }

      if (!receiverInfo.isConnected) {
        setStatusMessage("Connecting to GNSS receiver...");
        receiverInfo = await connectReceiver(accessCodeV2);
      }

      if (!receiverInfo.isConnected) {
        throw new Error(
          "GNSS receiver is not connected. Open Trimble Mobile Manager and connect to a GNSS receiver.",
        );
      }

      setStatusMessage("Opening position stream...");
      await openPositionStream();
    } catch (err) {
      closeWebSocket();
      setPosition(null);
      setConnectionState("disconnected");
      setError(err instanceof Error ? err.message : "Failed to connect to TMM.");
    }
  }, [closeWebSocket, openPositionStream]);

  useEffect(() => {
    return () => {
      closeWebSocket();
    };
  }, [closeWebSocket]);

  const isConnecting = connectionState === "connecting";
  const isConnected = connectionState === "connected";

  return (
    <main className="app">
      <header className="toolbar">
        <div className="toolbar-title">
          <h1>TMM API Web Sample</h1>
          <p>Live GNSS position from Trimble Mobile Manager</p>
        </div>
        <div className="toolbar-actions">
          <button type="button" onClick={() => void connect()} disabled={isConnecting || isConnected}>
            Connect
          </button>
          <button type="button" onClick={disconnect} disabled={!isConnecting && !isConnected}>
            Disconnect
          </button>
        </div>
      </header>

      <section className="map-panel">
        <MapView position={position} />
        {position && (
          <div className="position-overlay" aria-live="polite">
            <span>Latitude: {position.latitude.toFixed(8)}</span>
            <span>Longitude: {position.longitude.toFixed(8)}</span>
          </div>
        )}
      </section>

      {(statusMessage || error) && (
        <footer className="status-bar">
          {statusMessage && <p className="status-message">{statusMessage}</p>}
          {error && <p className="status-error">{error}</p>}
        </footer>
      )}
    </main>
  );
}
