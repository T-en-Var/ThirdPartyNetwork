# ThirdPartyNetwork

Monorepo: ASP.NET Core 9 backend (`backend/`) + Vue 3 / Vite frontend (`frontend/`).

## Local development

The whole stack (frontend, backend, Postgres, MinIO, pgAdmin, Portainer, and an
HTTPS reverse proxy) runs with a single `docker compose up`, on `https://tpn.test`.

HTTPS is required locally because the backend's refresh-token cookie is set with
`Secure=true; SameSite=None` — that combination only works over HTTPS. Browsers
silently drop it over plain `http://localhost`, which is why login "randomly"
doesn't stick on localhost.

### One-time setup (Windows)

1. Install [mkcert](https://github.com/FiloSottile/mkcert) if you don't have it:
```powershell
choco install mkcert
```
2. Install certificates. Running this multiple times should not break anything.
```powershell
.\infra\setup.ps1
```
3. Copy `.env.example` to `.env` in the repo root and fill in the values.
   Some don't need change.

### Running

```powershell
docker compose up --build
```

- Frontend: https://tpn.test
- Backend / API docs (Scalar): https://api.tpn.test/scalar
- File storage (MinIO's S3 API, what `MINIO_EXTERNAL_ENDPOINT` points at): https://cdn.tpn.test
- MinIO web console (admin UI, not proxied): http://localhost:9001
- pgAdmin: http://localhost:5050
- Portainer: https://localhost:9443

Both `frontend` and `backend` have **hot reload**:
- `frontend` runs `vite --host` with the source bind-mounted in — edits to
  `frontend/src` show up immediately via HMR, proxied through to `https://tpn.test`.
- `backend` runs `dotnet watch run` with the source bind-mounted in — saving a
  `.cs` file rebuilds and restarts the API automatically.

`node_modules`, and the backend's `bin`/`obj`, are kept in separate named Docker
volumes (not bind-mounted) so Linux-container build artifacts never clash with
anything built on the Windows host (e.g. from Visual Studio / Rider).

### Troubleshooting

- **Browser still shows "not secure"**: re-run `infra\setup.ps1` — it runs
  `mkcert -install`, which the browser needs to have picked up its CA from.
  Fully restart the browser afterwards.
- **Port 80/443 already in use**: something else on your machine (IIS, Skype,
  another proxy) is bound to it. Stop it, or change the `proxy` service's
  published ports in `docker-compose.yml`.
- **Hot reload not triggering**: this is a known rough edge of bind-mounting a
  Windows directory into a Linux container — both `vite.config.ts` and the
  backend container already force polling-based file watching
  (`CHOKIDAR_USEPOLLING` / `DOTNET_USE_POLLING_FILE_WATCHER`) to work around it.
  If it's still not working, restart the affected container.

## Production

`backend/docker-compose.yml` is the production backend stack (builds the
release `Dockerfile`, no dev/hot-reload tooling) and is unrelated to the local
dev compose file above — it isn't changed by any of this.
