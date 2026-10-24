# ADR-006: Data engines: PostgreSQL, MongoDB, Redis-compatible

- **Status:** Accepted (sponsor approval, 2026-10-24). Ordering, Inventory and Payment stay on PostgreSQL as written below; sponsor confirmed PostgreSQL for them (2026-10-24)
- **Date:** 2026-10-24
- **Reference:** `Project_Documentation/D-Technology-Architecture/D2 section 4`

## Context
Phase C (R-13) required every distinct database engine to be justified. Options: three engines, two engines (Catalogue on PostgreSQL JSON), or four (adding SQL Server for Ordering).

## Decision
Use **PostgreSQL** (Ordering, Inventory, Payment, Keycloak), **MongoDB** (Catalogue) and a **Redis-compatible store** (Basket, cache, limiter state, gateway sessions). SQL Server is not used.

## Rationale
Each engine fits its access pattern. On business criteria alone, two engines is slightly simpler and nearly as capable for the Catalogue. MongoDB is kept deliberately, with learning and portfolio value as the stated tie-breaker (consistent with ADR-004). SQL Server adds a fourth skill and licence cost without serving an objective.

## Consequences
- Three operational skills for four teams.
- Catalogue access stays behind a repository port, so moving it to PostgreSQL later is possible.
