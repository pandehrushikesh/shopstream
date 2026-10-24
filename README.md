# ShopStream

A production-style **.NET 10 microservices e-commerce platform**, built step by step as a hands-on refresher of API and distributed-systems design: authentication and authorization, API gateway (YARP), rate limiting, sagas, resilience, observability, and a React UI.

> Learning/portfolio project. Every major decision is recorded in [`docs/adr`](docs/adr).

## Scale target (design assumptions)

| Metric | Assumption |
|---|---|
| Daily active users | 1M, ~30 requests each, about 350 RPS average |
| Peak | 8x average, **~3,000 RPS at the gateway** |
| Orders | 200k/day, about 50/s peak, **500/s flash-sale burst** |
| Read:write | about 95:5, so catalog reads are cached, the order path is correct first |

## Architecture (target, per the approved TOGAF documentation)

The full design lives in [`Project_Documentation`](Project_Documentation/00-Index.md) (Phases Preliminary to H, all approved).

| Service | Responsibility | Store |
|---|---|---|
| Gateway (YARP) | Routing, sign-in as BFF, rate limiting, admission control | Redis-compatible (limiter state, sessions) |
| Identity | OIDC, roles, tokens | Keycloak + PostgreSQL |
| **Catalogue** | Products, categories | **MongoDB** |
| Basket | Temporary carts | Redis-compatible |
| Inventory | Stock, reservations | PostgreSQL |
| Ordering | Order lifecycle, checkout saga, outbox | PostgreSQL |
| Payment | Simulated PSP, idempotent ledger | PostgreSQL |
| Notification | Event consumer | none |

Messaging: RabbitMQ with MassTransit v8 (free fallback if needed). Telemetry: OpenTelemetry, Prometheus, Grafana, Loki, Tempo. Runtime: .NET 10 LTS on Kubernetes.

## Roadmap (transition architectures, see E1 in the documentation)

- [x] **TA-0 Foundation:** .NET 10, CI, local stack (gate G-0 passed 2026-10-24)
- [ ] TA-1 Platform and edge: cluster, observability, Keycloak, YARP gateway, rate limits
- [ ] **TA-2 Browse:** Catalogue service and React storefront (first stopping point)
- [ ] TA-3 Basket
- [ ] TA-4 Order path: messaging, Inventory, Payment, Ordering saga
- [ ] TA-5 Notification and retirement of the fictional monolith
- [ ] TA-6 Proven: load tests T-1 to T-8 and recovery drill

Spikes (time-boxed): SP-3 gateway limiting, SP-1 messaging library, SP-2 hot-item stock. See [`spikes/`](spikes/README.md).

## Repository layout

```
src/
  BuildingBlocks/
    ShopStream.BuildingBlocks        Result/Error, keyset pagination (no dependencies)
    ShopStream.BuildingBlocks.Web    ProblemDetails mapping, global exception handler
  Services/Catalog/
    ShopStream.Catalog.Domain        Entities + invariants (no infrastructure references)
    ShopStream.Catalog.Application   Use cases and ports (repository interfaces)
    ShopStream.Catalog.Infrastructure  MongoDB adapters, indexes, seeding, health check
    ShopStream.Catalog.Api           Minimal API, versioning, OpenAPI, Serilog
tests/
  ShopStream.Catalog.UnitTests         Domain + use-case tests (no I/O)
  ShopStream.Catalog.IntegrationTests  Real MongoDB via Testcontainers
deploy/docker-compose.yml            MongoDB, PostgreSQL, Redis, RabbitMQ, Seq, Keycloak, Catalog API
docs/adr/                            Architecture decision records
```

## Run it

Prerequisites: .NET 10 SDK, Docker. Local data stores: see [`docs/guides/local-data-stores-setup.md`](docs/guides/local-data-stores-setup.md).

```bash
# 1. Infrastructure only, then run the API from your IDE/CLI
docker compose -f deploy/docker-compose.yml up -d mongo seq
dotnet run --project src/Services/Catalog/ShopStream.Catalog.Api
#    Swagger UI: http://localhost:5101/swagger   (seeds ~10k products on first start)

# 2. Or everything in containers
docker compose -f deploy/docker-compose.yml up -d --build
#    Catalog API: http://localhost:5101/swagger   Seq: http://localhost:5341   RabbitMQ: http://localhost:15672

# Tests (integration tests need Docker running)
dotnet test
```

## Catalog API (v1)

| Method | Route | Notes |
|---|---|---|
| GET | `/api/v1/products?categoryId=&cursor=&limit=` | Keyset pagination, limit 1-100 (default 20) |
| GET | `/api/v1/products/{id}` | |
| POST | `/api/v1/products` | 201 + Location; duplicate SKU gives 409 |
| PUT | `/api/v1/products/{id}` | SKU is immutable |
| DELETE | `/api/v1/products/{id}` | |
| GET/POST | `/api/v1/categories` | |
| GET | `/health/live`, `/health/ready` | Liveness vs readiness (ready checks MongoDB) |

Errors are RFC 7807 `application/problem+json` with a stable `code` extension and a `traceId`.

## Interview talking points (Phase 1)

- **Polyglot persistence:** Mongo for catalog (flexible attributes, read-heavy) vs PostgreSQL for inventory (row locking and consistency). Cost: no cross-service joins. See ADR-001.
- **Keyset vs offset pagination:** stable under concurrent writes and constant cost per page, but no random page jump. See ADR-003.
- **Uniqueness without races:** the unique index decides SKU conflicts atomically, not a check-then-insert.
- **Liveness vs readiness:** liveness never checks dependencies, or a database blip would restart healthy pods.
- **Result type over exceptions** for expected failures; exceptions stay for the unexpected, handled globally.
- **Why no MediatR:** plain use-case classes, fewer dependencies, no licensing concerns.
