# D6: Technology Gap Analysis and Phase D Summary

| Field | Value |
|---|---|
| Phase | D |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | D1 to D5, C5, A5 |

## 1. Purpose
Compare baseline and target technology, record the technology requirements added in Phase D, update the risk position, list the decisions the sponsor must take, and hand off to Phases E and F.

## 2. Baseline technology (`[assumed]`, from A3)
Two virtual machines behind a load balancer, one .NET web application, one SQL Server database, in-memory sessions, no cache, no broker, manual deployment, server logs only.

## 3. Technology gap analysis

| ID | Area | Baseline | Target | Gap | Closure |
|---|---|---|---|---|---|
| GT-1 | Runtime | Application on an older .NET version | .NET 10 LTS (ADR-005) | Upgrade | Retarget, test |
| GT-2 | Hosting | Two VMs | Kubernetes across three zones (D1) | New platform | Adopt managed Kubernetes, build platform team skills |
| GT-3 | Data | One SQL Server | PostgreSQL, MongoDB, Redis-compatible (ADR-006) | New engines, data split | Per-wave migration (C5 section 5) |
| GT-4 | Messaging | None | RabbitMQ with outbox/inbox (ADR-007) | New | Adopt, MassTransit v8 with spike (OD-1) |
| GT-5 | Edge | Load balancer | CDN, WAF, gateway with rate limiting (D4) | New | Adopt, build gateway |
| GT-6 | Identity | In-application accounts | Keycloak with BFF (ADR-008) | Migration of accounts | Import, password reset or hash migration |
| GT-7 | Observability | Server logs | OpenTelemetry, SLOs, alerts (D5) | New | Adopt |
| GT-8 | Delivery | Manual | Pipeline, GitOps, canary (D5) | New | Build |
| GT-9 | Security | Perimeter only | Layered controls, scanning, signing (D3) | Many | Build incrementally with platform |
| GT-10 | Recovery | Unknown | Warm standby, RTO 30 / RPO 1 (D4) | New | Build and drill |
| GT-11 | Capacity | Not tested at peak | Verified by T-1 to T-8 (D4) | Missing evidence | Load-test programme |

## 4. Technology requirements added in Phase D

| Req | Requirement | Objective | Source |
|---|---|---|---|
| REQ-25 | The gateway enforces the limits in D4 section 4, answering 429 with Retry-After, and falls back safely if the shared counter store is unavailable | O1, O5 | D4 |
| REQ-26 | Each service validates tokens and scopes itself, independent of the gateway | O5 | D3 |
| REQ-27 | No secret is stored in source control, images or logs. Secrets come from a secret manager | O5 | D3 |
| REQ-28 | Only scanned, signed images with a software bill of materials are admitted to production | O5 | D3, D5 |
| REQ-29 | Workloads run as non-root with default-deny network policy | O5 | D3 |
| REQ-30 | Staging load tests T-1 to T-8 pass before the first production release | O1, O2, O3 | D4 |
| REQ-31 | A recovery drill meeting RTO 30 min and RPO 1 min is performed at least twice a year | O6 | D4 |
| REQ-32 | Every Tier 1 service has SLOs, dashboards and actionable alerts | O6 | D5 |
| REQ-33 | Schema, API and message changes follow expand and contract without downtime | O4 | D5 |
| REQ-34 | All infrastructure is defined as code and environments are reproducible | O4 | D1, D5 |
| REQ-35 | All components run on supported LTS versions at all times | O5, O6 | D2 |

Objective coverage across REQ-01 to REQ-35: O1 (REQ-01, 25, 30), O2 (REQ-04, 09, 11, 14, 19, 23, 30), O3 (REQ-02, 03, 09, 10, 12, 15, 16, 17, 18, 23, 30), O4 (REQ-06, 22, 33, 34), O5 (REQ-05, 08, 20, 25 to 29, 35), O6 (REQ-04, 07, 13, 19, 21, 24, 31, 32, 35).

New design decision DD-5 (price-change confirmation, D4 section 5) is carried into Ordering's contract.

## 5. Risk and issue updates (also in A5)

| ID | Item | L | I | Response |
|---|---|---|---|---|
| I-03 | The code spike targets .NET 8, which loses support on 10 Nov 2026 | n/a | Medium | Retarget to .NET 10 before further code |
| R-14 | Running on an unsupported runtime or library | Medium | High | TP-6, REQ-35, lifecycle watchlist (D2 section 6) |
| R-15 | MassTransit v8 patch window ends 2026 or .NET 10 incompatibility blocks the saga | Medium | High | Accepted by sponsor. Spike first, own messaging port, free fallback |
| R-16 | Payment provider rate limits cap offer size or cause reservation expiry | Medium | High | Offer-size rule in D4 section 2.3, confirm provider limit (DEP-03) |
| R-17 | Kubernetes and platform skills are thin | Medium | Medium | Managed Kubernetes, platform team T4 first, training |
| A-10 | Per-instance throughput rates and cache hit ratio in D4 are placeholders | n/a | n/a | Replaced by measurements from T-1 to T-3 |

## 6. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve D1 to D6 | **Approved 2026-10-24** |
| 2 | ADR-005: .NET 10 LTS, containers, Kubernetes, cloud-neutral | **Approved 2026-10-24** |
| 3 | ADR-006: three engines (PostgreSQL, MongoDB, Redis-compatible), no SQL Server | **Accepted 2026-10-24.** Ordering, Inventory, Payment confirmed on PostgreSQL |
| 4 | ADR-007 / OD-1: messaging library | **Decided 2026-10-24:** no paid licence. MassTransit v8, free alternative as fallback. Risk accepted (R-15) |
| 5 | ADR-008: gateway as BFF | **Approved 2026-10-24** |
| 6 | Planning assumptions in D4 (per-instance rates, 5-minute burst, limit values) | **Approved 2026-10-24** as placeholders to be measured |

## 7. Hand-off to Phases E and F

| To | Content |
|---|---|
| Phase E (Opportunities and Solutions) | Candidate work packages: platform foundation (cluster, pipeline, observability, identity, gateway, broker), Catalogue migration, Basket, saga trio (Ordering, Inventory, Payment), Notification, load-test programme, DR build. Spikes: messaging library (OD-1), hot-item stock strategy (C1 section 7), gateway rate-limiting implementation |
| Phase F (Migration Planning) | Wave order from C5 section 5, strangler routing at the gateway, data migration per wave, cut-over and rollback plans, change freeze rules |
| Phase G (Governance) | Definition of done (D5 section 4), compliance checklist from REQ-01 to REQ-35, ADR review rules |

## 8. Phase D exit criteria

| Criterion | Status |
|---|---|
| Platform and cloud-neutral approach defined (D1) | Approved |
| Technology choices recorded with licence and lifecycle checks (D2, ADR-005 to 008) | Approved |
| Security architecture and threat model (D3) | Approved |
| Capacity, resilience, rate limiting, recovery and verification plan (D4) | Approved |
| Observability and delivery architecture (D5) | Approved |
| Gaps, requirements and risks updated (D6, A5) | Approved |
| Sponsor decisions in section 6 taken | Pending |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
