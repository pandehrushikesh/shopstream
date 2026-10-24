# ADR-001: Microservices with database-per-service (polyglot persistence)

- **Status:** Provisional. Service decomposition validated by ADR-004 (Phase C). Choice of store per service remains provisional until Phase D
- **Date:** 2026-10-24

## Context
ShopStream is a learning/portfolio system that must demonstrate gateway routing, service-level authorization, rate limiting, sagas, and operating many moving parts. Its workloads differ sharply: a read-heavy catalog (~95% reads), a contention-heavy inventory, a transactional order lifecycle, and ephemeral carts.

## Decision
Split by business capability into independently deployable services. **Each service owns its database and nobody else touches it.** Pick the store per access pattern:

| Service | Store | Reason |
|---|---|---|
| Catalog | MongoDB | Flexible product attributes, read-heavy, document-shaped |
| Basket | Redis | TTL, in-memory speed |
| Inventory | PostgreSQL | Row locking / optimistic concurrency on stock |
| Ordering | SQL Server | Transactions, outbox table, saga state |
| Payment | PostgreSQL | Idempotency-key store |
| Identity | Keycloak (PostgreSQL) | Managed, see ADR-002 |

## Consequences
- (+) Independent scaling and deployment; failures are contained; each store fits its workload.
- (+) Forces explicit contracts (HTTP, events) between services.
- (-) No cross-service joins or ACID transactions: we need sagas, the outbox pattern and idempotent consumers (Phases 4-5).
- (-) Much higher operational cost (observability, deployment, local dev). Mitigated with Docker Compose now, Aspire/Kubernetes later.
- (-) For a real product of this size, a modular monolith would often be the better first step. We choose microservices here deliberately to exercise the concepts, and we say so.
