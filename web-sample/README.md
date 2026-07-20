# TMM API Web Sample

A web-based sample application with a React frontend and an Express REST API backend for version 2 access code generation.

## Prerequisites

- [Node.js](https://nodejs.org/) 20 or later
- [pnpm](https://pnpm.io/) 9 or later

## Getting Started

From the repository root:

```bash
pnpm install
pnpm dev
```

This starts both services in parallel:

| Service | URL |
|---------|-----|
| Frontend (Vite) | http://localhost:5173 |
| Backend (Express) | http://localhost:3000 |

The Vite dev server proxies `/api` requests to the backend.

## Project Structure

```
web-sample/
  frontend/   React + Vite UI
  backend/    Express REST API
```

## Backend Configuration

Copy `web-sample/backend/.env.example` to `web-sample/backend/.env` and set your application ID:

```env
APP_ID=your-application-guid
```

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/health` | Health check |
| PUT | `/api/tmmPublicKey` | Set the RSA public key as JWK+JSON (`kty`, `n`, `e`) for v2 access codes |
| GET | `/api/tmmAccessCodeV2` | Generate a version 2 access code using `APP_ID` from `.env` |

## Usage

1. Set `APP_ID` in `web-sample/backend/.env`.
2. Run `pnpm dev` from the repository root.
3. Open http://localhost:5173.
4. Submit the TMM RSA public key via **PUT /api/tmmPublicKey**.
5. Use **GET /api/tmmAccessCodeV2** to generate an access code.

Full TMM API documentation is available at https://developer.trimble.com/docs/mobile-manager
