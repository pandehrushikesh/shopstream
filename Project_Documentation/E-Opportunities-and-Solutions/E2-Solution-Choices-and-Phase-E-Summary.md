# E2: Solution Choices and Phase E Summary

| Field | Value |
|---|---|
| Phase | E |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | E1 (approved), D2, D5, D6, existing code spike, ADR-001 to ADR-008 |

## 1. Build, buy or reuse

Rule: build only where building is the learning goal or where no suitable component exists (PR-02, PR-10). Every adopted component passed the licence and lifecycle check in D2.

| Area | Choice | Why |
|---|---|---|
| Identity | **Reuse** Keycloak (ADR-002) | Standards-based, open source, no custom auth code |
| Gateway | **Build on** YARP with ASP.NET Core rate limiting (ADR-008) | Thin configuration and policy code, which is the interview topic |
| Broker | **Reuse** RabbitMQ (ADR-007) | Open source, fits volume |
| Saga, outbox, inbox | **Reuse** MassTransit v8, fallback free library or thin layer (ADR-007, SP-1) | Avoids hand-written reliability code unless the spike fails |
| Payment provider | **Simulate** with an adapter that follows real PSP semantics (A-12) | No real money, same failure modes (BR-09 to BR-12) |
| Observability | **Reuse** OpenTelemetry, Prometheus, Grafana, Loki, Tempo (D5) | Open standards, cloud-neutral |
| CI/CD, IaC | **Reuse** GitHub Actions, GitOps, OpenTofu (D5) | Free for public repositories, open source |
| Load testing | **Reuse** k6 (D4) | Scriptable, repeatable |
| Business services | **Build** Catalogue, Basket, Inventory, Payment, Ordering, Notification | Core of the learning goal |
| Storefront | **Build** React UI | Needed for the end-to-end story |
| Existing code spike | **Reuse selectively** (section 2) | Avoid throwing away working ideas |

## 2. What happens to the existing code spike

The Catalogue spike was written before the architecture was decided and has never been compiled. Treatment:

| Part | Decision |
|---|---|
| Result and keyset-paging building blocks | Keep. Review against ADR-003 |
| Catalogue domain, application, MongoDB infrastructure | Keep as the starting point of WP-06 after it compiles on .NET 10 |
| Seeder (10k products), health checks, tests | Keep |
| Targets (net8.0), package versions | Replace in WP-00 (.NET 10, current package versions) |
| Docker compose file | Already updated (MongoDB 8.0). Extend in WP-01 |
| README roadmap | Rewrite to follow the ADM and the transition architectures |

WP-00 therefore has an explicit exit check: the solution restores, builds and passes unit tests under .NET 10 in CI. Until that passes, the spike is not regarded as working code.

## 3. Implementation and verification approach

| Topic | Approach |
|---|---|
| Branching | Trunk-based on `main`, short-lived branches, pull requests even for a single developer (builds the habit and the audit trail) |
| Definition of done | D5 section 4 checklist per package |
| Architecture compliance | ADR for each significant decision. Review at each transition architecture (Phase G) |
| Tests | Unit, integration with Testcontainers, contract tests at service boundaries, then T-1 to T-8 at WP-14 |
| Documentation | Keep ADRs and this folder current. A package is not done until its requirement rows in the register are updated |
| Spike handling | Code under `spikes/`, results recorded as ADR notes, then deleted or folded in |

## 4. Gaps, risks and assumptions

| ID | Type | Statement | Response |
|---|---|---|---|
| R-20 | Risk | Un-compiled spike hides build errors that surface late | WP-00 exit check, CI from day one |
| R-21 | Risk | Spike results contradict D4 sizing assumptions (A-10) | Change control: update D4 and, if needed, A3 |
| R-22 | Risk | Scope growth beyond TA-2 stalls the project | TA-2 is a complete stopping point; later TAs optional |

## 5. Phase E exit criteria

| Criterion | Status |
|---|---|
| Work packages with dependencies defined (E1) | Approved |
| Spikes chartered with pass/fail criteria (E1) | Approved |
| Transition architectures defined (E1) | Approved |
| Build, buy or reuse decided (E2) | Approved |
| Existing code treated explicitly (E2) | Approved |
| Risks and assumptions updated (A5) | Done (A5 v1.3) |

## 6. Hand-off to Phase F

| Item | Content |
|---|---|
| Wave and TA ordering | E1 section 4 against C5 section 5 |
| To plan in Phase F | Strangler routing at the gateway per wave, data migration steps, cut-over and rollback for each wave, freeze rules, coexistence of Classic and new services, the paper rehearsal of monolith retirement (WP-16) |
| Early start permitted | WP-00 and WP-01 (foundation) do not depend on Phase F and can begin when the sponsor chooses |

## 7. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Build, buy or reuse choices (section 1) | **Approved** |
| 2 | Treatment of the existing code spike (section 2) | **Approved** |
| 3 | Trunk-based development with pull requests (section 3) | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
