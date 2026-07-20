import {
  constants,
  createPublicKey,
  type KeyObject,
  publicEncrypt,
} from "node:crypto";

let publicKey: KeyObject | null = null;

export interface RsaPublicJwk {
  kty: string;
  n: string;
  e: string;
  [key: string]: unknown;
}

export function setPublicKey(jwk: RsaPublicJwk): void {
  if (jwk.kty !== "RSA") {
    throw new Error("Only RSA JWK keys are supported.");
  }

  if (!jwk.n?.trim() || !jwk.e?.trim()) {
    throw new Error("JWK must include n and e.");
  }

  publicKey = createPublicKey({
    key: jwk,
    format: "jwk",
  });
}

export function generateAccessCodeV2(appId: string, utcTime: Date): string {
  if (!publicKey) {
    throw new Error("public key not set");
  }

  const lowercaseId = appId.toLowerCase();
  const iso8601Time = utcTime.toISOString().split('.')[0] + 'Z';
  const plaintextAccessCode = `${lowercaseId} ${iso8601Time}`;
  const encryptedBytes = publicEncrypt(
    {
      key: publicKey,
      padding: constants.RSA_PKCS1_OAEP_PADDING,
      oaepHash: "sha256",
    },
    Buffer.from(plaintextAccessCode, "utf8"),
  );

  return encryptedBytes.toString("base64");
}

export function generateAccessCodeV2Now(appId: string): string {
  return generateAccessCodeV2(appId, new Date());
}
