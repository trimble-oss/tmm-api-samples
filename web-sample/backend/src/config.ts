function requireEnv(name: string): string {
  const value = process.env[name]?.trim();
  if (!value) {
    throw new Error(`${name} is not configured. Set it in web-sample/backend/.env`);
  }

  return value;
}

export function getAppId(): string {
  return requireEnv("APP_ID");
}
