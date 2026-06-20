# Cash & Carry POS — DevOps Portfolio

A sample **point-of-sale** system built to demonstrate a full DevOps workflow: an
Angular frontend, two **.NET 10** microservices, PostgreSQL (database-per-service),
containerization, Kubernetes manifests, and a CI pipeline.

## Architecture

```
                         ┌─────────────────────────────┐
   Browser ──────────────►  Frontend (Angular + nginx)  │  API gateway
                         └───────────┬─────────────────┘
                       /api/products │ /api/categories      /api/sales
                                     ▼                            ▼
                    ┌────────────────────────┐     ┌────────────────────────┐
                    │   Catalog Service       │◄────│    Sales Service        │
                    │   (.NET 10 Web API)     │ HTTP│    (.NET 10 Web API)    │
                    │   products · stock      │     │    sales · receipts     │
                    └───────────┬────────────┘     └───────────┬────────────┘
                                │ catalogdb                     │ salesdb
                                ▼                               ▼
                    ┌───────────────────────────────────────────────────────┐
                    │                    PostgreSQL                          │
                    └───────────────────────────────────────────────────────┘
```

- **Catalog Service** owns products, categories and **stock levels** (the single source
  of truth for inventory). Database: `catalogdb`.
- **Sales Service** rings up transactions. At checkout it calls the Catalog over HTTP to
  validate products/prices and **decrement stock**, then persists the sale and receipt.
  Database: `salesdb`.
- **Frontend** is a touch-friendly POS terminal. The nginx image also acts as the API
  gateway, routing each resource path to the owning service (no CORS in production).

Each service owns its own database and applies **EF Core migrations on startup**. The
Catalog seeds a small demo catalogue on first run.

## Tech stack

| Layer        | Technology                                              |
|--------------|---------------------------------------------------------|
| Frontend     | Angular 20 (standalone components, signals), nginx      |
| Backend      | .NET 10 / ASP.NET Core Web API, EF Core 10              |
| Database     | PostgreSQL 16 (one database per service)                |
| Container    | Docker (multi-stage builds), Docker Compose             |
| Orchestration| Kubernetes (Deployments, Services, Ingress, probes)     |
| CI           | GitHub Actions (build + test + image build)             |

## Run it locally

### Option A — Docker Compose (whole stack)

```bash
docker compose -f deploy/docker-compose.yml up --build
```

| URL                                | What                         |
|------------------------------------|------------------------------|
| http://localhost:8080              | POS terminal (frontend)      |
| http://localhost:5001/swagger      | Catalog API (Swagger UI)     |
| http://localhost:5002/swagger      | Sales API (Swagger UI)       |

### Option B — Kubernetes

```bash
# Build images and load them into your cluster (kind/minikube), then:
kubectl apply -k deploy/k8s
kubectl -n pos get pods
# Add "127.0.0.1 pos.local" to your hosts file, then browse http://pos.local
```

### Option C — Run the pieces directly (dev)

```bash
# 1. A Postgres with the two databases
docker run -d --name pos-pg -p 5432:5432 \
  -e POSTGRES_USER=pos -e POSTGRES_PASSWORD=pos -e POSTGRES_DB=pos postgres:16-alpine
docker exec pos-pg psql -U pos -c "CREATE DATABASE catalogdb;" -c "CREATE DATABASE salesdb;"

# 2. Backend (separate terminals)
dotnet run --project services/catalog-service --urls http://localhost:5001
dotnet run --project services/sales-service   --urls http://localhost:5002

# 3. Frontend (proxies /api to the services — see frontend/proxy.conf.json)
cd frontend && npm start    # http://localhost:4200
```

## Repository layout

```
services/
  catalog-service/   Pos.Catalog.Api  — products, categories, stock
  sales-service/     Pos.Sales.Api    — sales, receipts, Catalog client
  tests/             xUnit unit tests
frontend/            Angular POS terminal + nginx gateway
deploy/
  docker-compose.yml
  postgres/          per-service DB init script
  k8s/               namespace, postgres, deployments, services, ingress
.github/workflows/   ci.yml
```

See [CLAUDE.md](CLAUDE.md) for build/test commands and architecture notes.
