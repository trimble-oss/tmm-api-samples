import { FormEvent, useState } from "react";
import "./App.css";

export default function App() {
  const [publicKeyJson, setPublicKeyJson] = useState(
    '{\n  "kty": "RSA",\n  "n": "",\n  "e": "AQAB"\n}',
  );
  const [healthStatus, setHealthStatus] = useState("");
  const [publicKeyStatus, setPublicKeyStatus] = useState("");
  const [accessCodeV2, setAccessCodeV2] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function checkHealth() {
    setError("");
    setHealthStatus("");

    try {
      const response = await fetch("/api/health");
      const data = await response.json();
      setHealthStatus(JSON.stringify(data, null, 2));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Health check failed.");
    }
  }

  async function submitPublicKey(event: FormEvent) {
    event.preventDefault();
    setError("");
    setPublicKeyStatus("");
    setLoading(true);

    try {
      const jwk = JSON.parse(publicKeyJson) as unknown;
      const response = await fetch("/api/tmmPublicKey", {
        method: "PUT",
        headers: { "Content-Type": "application/jwk+json" },
        body: JSON.stringify(jwk),
      });
      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error ?? `Failed to set public key (${response.status})`);
      }

      setPublicKeyStatus(JSON.stringify(data, null, 2));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to set public key.");
    } finally {
      setLoading(false);
    }
  }

  async function fetchAccessCodeV2() {
    setError("");
    setAccessCodeV2("");
    setLoading(true);

    try {
      const response = await fetch("/api/tmmAccessCodeV2");
      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.error ?? `Failed to get access code (${response.status})`);
      }

      setAccessCodeV2(JSON.stringify(data, null, 2));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to get access code.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="app">
      <header>
        <h1>TMM API Web Sample</h1>
        <p>
          React frontend with an Express REST API backend for version 2 access code generation.
        </p>
      </header>

      <section className="card">
        <h2>Backend Health</h2>
        <div className="actions">
          <button type="button" onClick={() => void checkHealth()}>
            Check /api/health
          </button>
        </div>
        {healthStatus && <pre className="status success">{healthStatus}</pre>}
      </section>

      <section className="card">
        <h2>Public Key</h2>
        <p>Submit the TMM RSA public key in JWK+JSON format.</p>
        <form onSubmit={(event) => void submitPublicKey(event)}>
          <div className="field">
            <label htmlFor="publicKeyJson">JWK+JSON</label>
            <textarea
              id="publicKeyJson"
              value={publicKeyJson}
              onChange={(event) => setPublicKeyJson(event.target.value)}
              rows={8}
              required
            />
          </div>
          <div className="actions">
            <button type="submit" disabled={loading}>
              PUT /api/tmmPublicKey
            </button>
          </div>
        </form>
        {publicKeyStatus && <pre className="status success">{publicKeyStatus}</pre>}
      </section>

      <section className="card">
        <h2>Access Code V2</h2>
        <p>Generate a version 2 access code using the configured public key and APP_ID.</p>
        <div className="actions">
          <button type="button" onClick={() => void fetchAccessCodeV2()} disabled={loading}>
            GET /api/tmmAccessCodeV2
          </button>
        </div>
        {accessCodeV2 && <pre className="status success">{accessCodeV2}</pre>}
      </section>

      {error && <pre className="status error">{error}</pre>}
    </main>
  );
}
