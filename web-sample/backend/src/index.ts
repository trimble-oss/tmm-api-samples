import "dotenv/config";
import express, { type NextFunction, type Request, type Response } from "express";
import { generateAccessCodeV2Now, setPublicKey } from "./accessCodeV2.js";
import { getAppId } from "./config.js";

const app = express();
const port = Number(process.env.PORT ?? 3000);

app.use(express.json({ type: ["application/json", "application/jwk+json"] }));

app.get("/api/health", (_req, res) => {
  res.json({ status: "ok" });
});

app.put("/api/tmmPublicKey", (req, res) => {
  const jwk = req.body as {
    kty?: string;
    n?: string;
    e?: string;
  };

  if (!jwk?.kty || !jwk.n || !jwk.e) {
    res.status(400).json({
      error: "Request body must be an RSA public key in JWK format with kty, n, and e.",
    });
    return;
  }

  try {
    setPublicKey({ kty: jwk.kty, n: jwk.n, e: jwk.e });
    res.json({ success: true });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to set public key.";
    res.status(400).json({ error: message });
  }
});

app.get("/api/tmmAccessCodeV2", (_req, res) => {
  try {
    const accessCodeV2 = generateAccessCodeV2Now(getAppId());
    res.json({ accessCodeV2 });
  } catch (error) {
    const message = error instanceof Error ? error.message : "Failed to generate access code.";
    const status = message.includes("not configured") || message.includes("not set") ? 400 : 500;
    res.status(status).json({ error: message });
  }
});

app.use((error: unknown, _req: Request, res: Response, next: NextFunction) => {
  if (error instanceof SyntaxError && "body" in error) {
    res.status(400).json({ error: "Request body must be valid JSON." });
    return;
  }

  next(error);
});

app.listen(port, () => {
  console.log(`Backend listening on http://localhost:${port}`);
});
