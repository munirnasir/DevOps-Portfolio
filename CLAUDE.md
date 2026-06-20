# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

A sample **Cash & Carry POS**: Angular 20 frontend + two **.NET 10** microservices
(Catalog, Sales) on PostgreSQL, with Docker Compose, Kubernetes, and GitHub Actions CI.

## Toolchain (this machine)

- **.NET 10 SDK lives at `C:\dotnet`**, not `C:\Program Files\dotnet` (which only has 8/9).
  `dotnet` resolves via the user `PATH`; EF tooling needs `DOTNET_ROOT=C:\dotnet` (both
  persisted at user scope). A *new* shell inherits these; if `dotnet --version` shows 9.x,
  prepend `C:\dotnet` to `PATH`.
- **Angular CLI is v20**, not latest — Node 22.18 is below the v22 CLI's minimum.
- The solution file is **`services/Pos.slnx`** (XML format, default in .NET 10) — not `.sln`.

## Common commands

Backend (from repo root):
- Build:        `dotnet build services/Pos.slnx -c Release`
- Test all:     `dotnet test services/Pos.slnx`
- Single test:  `dotnet test services/tests/Pos.Sales.Tests --filter FullyQualifiedName~SaleCalculatorTests`
- Run a service:`dotnet run --project services/catalog-service --urls http://localhost:5001`
- Add migration:`dotnet-ef migrations add <Name> --project services/catalog-service --context CatalogDbContext`
                (Sales: `--project services/sales-service --context SalesDbContext`)

Frontend (from `frontend/`):
- Dev server:   `npm start`   (proxies `/api` to the services via `proxy.conf.json`)
- Build:        `npm run build`
- Unit tests:   `npm test`

Whole stack:
- Compose:      `docker compose -f deploy/docker-compose.yml up --build`
- Kubernetes:   `kubectl apply -k deploy/k8s`

## Architecture (the cross-cutting parts)

- **Two bounded contexts.** Catalog owns products + inventory and is the single source of
  truth for stock. Sales orchestrates checkout: it calls Catalog over HTTP via the typed
  `CatalogClient` to validate products/prices and decrement stock. There is **no distributed
  transaction** — `SalesController` manually **compensates** (re-adds stock) if a later line
  fails. Sale lines snapshot product data so historical receipts never change.
- **Money math is server-authoritative.** `SaleCalculator` (pure, unit-tested) computes line
  and sale totals; the Angular component mirrors the math only for instant UX feedback.
- **Database-per-service.** `catalogdb` and `salesdb` on one Postgres. Each service applies
  **EF Core migrations on startup** (`DatabaseStartup`, with a retry loop for a cold DB);
  Catalog seeds a demo catalogue. Design-time `IDesignTimeDbContextFactory` classes let the
  EF CLI add migrations without booting the web host.
- **API gateway routes by resource path:** `/api/products` & `/api/categories` → Catalog,
  `/api/sales` → Sales. This is implemented **twice**: the frontend nginx (compose/standalone)
  and the Kubernetes Ingress. The frontend always calls **relative `/api/*`** URLs so one
  build works in every environment (no CORS in prod).
- **Config / env vars:** `ConnectionStrings__CatalogDb` / `__SalesDb`;
  `Services__CatalogBaseUrl` (Sales→Catalog); `CATALOG_HOST` / `SALES_HOST` (frontend nginx
  envsubst). Containers listen on **8080**; host maps **5001** (catalog), **5002** (sales),
  **8080** (frontend).
- **Health endpoints:** `/health/live` (liveness, no checks) and `/health/ready` (DB check),
  wired to K8s probes. The HTTP server only starts accepting traffic after startup migration
  finishes, so a `startupProbe` covers the cold-start window.
