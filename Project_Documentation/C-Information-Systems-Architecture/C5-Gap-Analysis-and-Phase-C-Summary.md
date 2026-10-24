# C5: Gap Analysis and Phase C Summary

| Field | Value |
|---|---|
| Phase | C |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | C1 to C4, B6, A5 |

## 1. Purpose
Compare baseline and target data and application architectures, add the architecture requirements that Phase C created, update risks, and hand off to Phase D.

## 2. Data architecture gaps

| ID | Baseline | Target | Gap | Closure |
|---|---|---|---|---|
| GD-1 | One shared database and schema | One owner per entity (C1) | Entities and ownership must be separated | Split by owner, replicas through events |
| GD-2 | Stock as a column updated in the order transaction | Reservation entity with expiry and commit | New entity and rules | New in Inventory |
| GD-3 | No price snapshot | Snapshot in the order (BR-06) | Missing | New in Ordering |
| GD-4 | Payment data mixed into orders | Separate restricted store, token only | Mixed data classes | Separate in Payment |
| GD-5 | No idempotency record | Checkout key and inbox | Missing | New in Ordering and consumers |
| GD-6 | Sequential database ids | Global, service-generated ids | Region readiness | Change id strategy for new data |
| GD-7 | Personal data in logs | Identifiers only | Privacy risk | Logging standard (T4) |

## 3. Application architecture gaps

| ID | Baseline | Target | Gap | Closure |
|---|---|---|---|---|
| GA-1 | One deployable | Six domain services plus platform (ADR-004) | Decomposition | Strangler migration (Phase F) |
| GA-2 | In-process calls and shared transaction | APIs, messages, saga | New integration style | New |
| GA-3 | Synchronous payment inside the order transaction | Orchestrated saga with compensation | Redesign of checkout | New in Ordering |
| GA-4 | No broker | Broker, outbox, inbox | New platform component | Adopt in Phase D |
| GA-5 | No gateway or central identity | Gateway and identity provider | New platform components | Adopt |
| GA-6 | Sessions in server memory | Stateless services, basket in its own store | State externalised | New in Basket |
| GA-7 | No contract management | Versioned contracts and contract tests | Missing | New process (PR-05) |

## 4. Architecture requirements added in Phase C

| Req | Requirement | Objective | Source |
|---|---|---|---|
| REQ-16 | Place order is idempotent through a per-customer checkout key | O3 | C4 section 7 |
| REQ-17 | A state change and the event announcing it are committed atomically (outbox) | O3 | C1 section 5 |
| REQ-18 | Every message consumer is safe to repeat (inbox) | O3 | C1, C4 |
| REQ-19 | Services degrade as specified in C2 section 7. Tier 3 failures never block orders | O2, O6 | C2 |
| REQ-20 | Data classes are enforced. Logs and messages carry identifiers only | O5 | C1 section 8 |
| REQ-21 | Design stays multi-region-ready: global ids, UTC, no cross-service transactions | O6 | C1 section 12 |
| REQ-22 | APIs and messages are versioned, additive within a version, covered by contract tests | O4 | C4 section 9 |
| REQ-23 | Inventory supports reservations for hot items at the burst rate without oversell, shown by load test | O2, O3 | C1 section 7 |
| REQ-24 | Every service exposes liveness and readiness checks and propagates correlation ids | O6 | C2, C1 |

Objective coverage check (all requirements REQ-01 to REQ-24): O1 (REQ-01), O2 (REQ-04, 09, 11, 14, 19, 23), O3 (REQ-02, 03, 09, 10, 12, 15, 16, 17, 18, 23), O4 (REQ-06, 22), O5 (REQ-05, 08, 20), O6 (REQ-04, 07, 13, 19, 21, 24). O1 is covered by a single requirement; per-service performance requirements are added in Phase D.

## 5. Indicative migration order (detail in Phase F)

| Wave | Content | Reason |
|---|---|---|
| 0 | Platform: gateway, identity, observability, pipeline | Prerequisite for everything (CAP-09 to CAP-12) |
| 1 | Catalogue | Read-heavy, low consistency risk, immediate scaling benefit |
| 2 | Basket | Removes session coupling |
| 3 | Inventory, Payment, Ordering together | These form the saga and cannot be split safely one by one |
| 4 | Notification, retire the monolith | Low risk, last |

## 6. Risk and assumption updates (also in A5)

| ID | Item |
|---|---|
| R-11 | Hot-item stock contention limits flash-sale throughput (C1 section 7, REQ-23) |
| R-12 | Saga races (reservation expiry vs late payment) produce wrong states (C4 section 8) |
| R-13 | Too many distinct database engines for four teams (C1 section 3) |
| A-08 | Retention periods and saga time limits in C1 and C4 are assumptions until reviewed by Finance and Security |

## 7. Hand-off to Phase D (Technology Architecture)

| Topic | Decision needed in Phase D |
|---|---|
| Hosting and platform | Container platform, region pair in India, recovery design (RTO 30 min, RPO 1 min) |
| Data engines | Choose products per store class. Justify each engine (R-13). SQL Server vs PostgreSQL for Ordering is open |
| Messaging | Broker product, delivery guarantees, dead-letter handling, ordering guarantees |
| Identity | Confirm the product (ADR-002 provisional). Token lifetimes, scopes per service |
| Gateway | Product, rate limiting strategy (local and distributed), caching |
| Security | Threat model, secrets, network segmentation, service-to-service trust |
| Observability | Logging, metrics, tracing, alert design from B2 metrics |
| Capacity | Convert section 6 of C2 into sizing. Load-test plan to validate A-01 and REQ-23 |
| Delivery | Pipeline, environments, deployment strategy, infrastructure as code |

## 8. Phase C exit criteria

| Criterion | Status |
|---|---|
| Data entities have exactly one owner (C1) | Met (approved 2026-10-24) |
| Application portfolio and responsibilities defined (C2) | Met (approved 2026-10-24) |
| Alternatives to services evaluated and decision recorded (C3, ADR-004) | Met (approved 2026-10-24) |
| Interfaces, messages and the checkout saga specified (C4) | Met (approved 2026-10-24) |
| Gaps, new requirements and risks recorded (C5, A5) | Met (approved 2026-10-24) |
| Open design decisions listed with owners and phase (C4 section 10) | Met |

## 9. Decisions requested
1. Approve C1 to C5 and ADR-004.
2. Review the assumed retention periods (C1 section 9) and saga time limits (C4 section 6).
3. Answer **Q6**: is there any hosting constraint for the fictional organisation (for example a mandated cloud or on-premises Kubernetes), or should Phase D stay cloud-neutral?

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
