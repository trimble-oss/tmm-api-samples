import { TMM_REST_BASE_URL } from "./constants";
import type { ReceiverInfo, RsaPublicJwk } from "./types";

function tmmUrl(path: string): string {
  return `${TMM_REST_BASE_URL}${path}`;
}

async function readErrorMessage(response: Response): Promise<string> {
  try {
    const data = (await response.json()) as { error?: string; detail?: string };
    return data.error ?? data.detail ?? `Request failed (${response.status})`;
  } catch {
    return `Request failed (${response.status})`;
  }
}

export async function fetchTmmPublicKey(): Promise<RsaPublicJwk> {
  const response = await fetch(tmmUrl("/api/v1/publicKey/"), {
    headers: { Accept: "application/json" },
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  return (await response.json()) as RsaPublicJwk;
}

export async function submitPublicKeyToBackend(jwk: RsaPublicJwk): Promise<void> {
  const response = await fetch("/api/tmmPublicKey", {
    method: "PUT",
    headers: { "Content-Type": "application/jwk+json" },
    body: JSON.stringify(jwk),
  });

  if (!response.ok) {
    const data = (await response.json()) as { error?: string };
    throw new Error(data.error ?? `Failed to set public key (${response.status})`);
  }
}

export async function fetchAccessCodeV2FromBackend(): Promise<string> {
  const response = await fetch("/api/tmmAccessCodeV2");
  const data = (await response.json()) as { accessCodeV2?: string; error?: string };

  if (!response.ok) {
    throw new Error(data.error ?? `Failed to get access code (${response.status})`);
  }

  if (!data.accessCodeV2) {
    throw new Error("Backend did not return an access code.");
  }

  return data.accessCodeV2;
}

function authorizedHeaders(accessCodeV2: string): HeadersInit {
  return {
    Accept: "application/json",
    Authorization: `AccessCodeV2 ${accessCodeV2}`,
  };
}

export async function fetchReceiverInfo(accessCodeV2: string): Promise<ReceiverInfo> {
  const response = await fetch(tmmUrl("/api/v1/receiver/"), {
    headers: authorizedHeaders(accessCodeV2),
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  return (await response.json()) as ReceiverInfo;
}

export async function connectReceiver(accessCodeV2: string): Promise<ReceiverInfo> {
  const response = await fetch(tmmUrl("/api/v1/receiver/"), {
    method: "PUT",
    headers: {
      ...authorizedHeaders(accessCodeV2),
      "Content-Type": "application/json",
    },
    body: JSON.stringify({ isConnected: true }),
  });

  if (!response.ok) {
    throw new Error(await readErrorMessage(response));
  }

  return (await response.json()) as ReceiverInfo;
}
