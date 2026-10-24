# A5: RAID Log (Risks, Assumptions, Issues, Dependencies)

| Field | Value |
|---|---|
| Phase | A (living document, maintained through all phases) |
| Version | 1.0 (Approved baseline 2026-10-24; living document) |
| Date | 2026-10-24 |
| Owner | Lead Architect |

Scoring: Likelihood and Impact on a scale of Low / Medium / High.

## Risks

| ID | Risk | L | I | Mitigation | Owner |
|---|---|---|---|---|---|
| R-01 | Over-engineering: microservices add cost not justified by the objectives | High | High | Apply PR-02. Compare a modular monolith against O1 to O6 in Phase C/D and record the result as an ADR | Lead Architect |
| R-02 | Scope creep into out-of-scope areas (shipping, returns, marketplace) | Medium | Medium | Scope list in A3. Changes need an explicit decision | Lead Architect |
| R-03 | Capacity assumptions (3,000 req/s, 500 orders/s) are wrong or untested | Medium | High | Treat as assumptions. Validate by load-test spikes in Phase D. Revise objectives if needed | Lead Architect |
| R-04 | Saga and distributed-consistency complexity causes incorrect states | Medium | High | Model the checkout saga formally in Phase C. Failure-injection tests. Idempotency (PR-07) | Lead Architect |
| R-05 | Operational overhead of many components exceeds team capacity | Medium | Medium | Limit service count. Automate (PR-10). Start with a minimal platform | Engineering leads |
| R-06 | Security gaps at service boundaries | Medium | High | Threat model in Phase D. Defence in depth (PR-09) | Security officer |
| R-07 | Migration from the monolith disrupts live trading | Medium | High | Incremental migration with a strangler approach in Phase F. Rollback paths | Lead Architect |
| R-08 | Earlier code spike is treated as final design | Medium | Medium | ADRs marked Provisional. Index states its status | Lead Architect |
| R-09 | Business rules (reservation time, price snapshot, per-customer limit) are wrong for real trading | Low | Medium | Confirmed by sponsor on 2026-10-24. Revisit if real trading data contradicts them | Product owner |
| R-11 | Hot-item stock contention limits flash-sale throughput | High | High | Choose a hot-key strategy by load test (REQ-23, C1 section 7) | Team T3 |
| R-12 | Saga races produce wrong order states | Medium | High | Rules in C4 section 8, failure-injection tests | Team T2 |
| R-13 | Too many database engines for four teams | Medium | Medium | Justify each engine in Phase D. Consider consolidation | Lead Architect |
| R-14 | Running on an unsupported runtime or library | Medium | High | TP-6, REQ-35, lifecycle watchlist (D2) | Lead Architect |
| R-15 | MassTransit v8 patch window ends 2026 or .NET 10 incompatibility blocks the saga | Medium | High | **Accepted by sponsor 2026-10-24.** Own messaging port, spike in Phase E, free fallback | Team T2 |
| R-16 | Payment provider limits cap offer size or cause reservation expiry | Medium | High | Offer-size rule (D4 section 2.3), confirm limit (DEP-03) | Team T3 |
| R-17 | Kubernetes and platform skills are thin | Medium | Medium | Managed Kubernetes, T4 first, training | Operations lead |
| R-18 | One person plays four teams, so work packages serialise and the plan stretches | High | Medium | Small packages, transition architectures as stopping points, TA-2 first milestone | Lead Architect |
| R-19 | Platform learning (Kubernetes, Keycloak, OpenTofu) consumes time before business features | Medium | Medium | Minimal WP-02, time-box, templates | Team T4 |
| R-20 | Un-compiled spike hides build errors that surface late | Medium | Low | WP-00 exit check, CI from day one | Team T4 |
| R-21 | Spike results contradict D4 sizing assumptions | Medium | Medium | Change control: update D4 and, if needed, A3 | Lead Architect |
| R-22 | Scope growth beyond TA-2 stalls the project | Medium | Medium | TA-2 is a complete stopping point | Lead Architect |
| R-23 | After days of live trading in wave 3, rolling back is no longer possible, only fixing forward | Medium | High | Staged product groups, daily reconciliation, rehearsed early rollback | Team T3 |
| R-24 | Classic password hash formats cannot be migrated | Medium | Medium | Fallback reset email, communication plan | Team T4 |
| R-25 | Shadow comparison noise hides real defects | Medium | Medium | Agree tolerances in advance | Team T4 |
| R-26 | Self-review becomes a rubber stamp | Medium | Medium | Checklist-based, paused review, evidence links required | Lead Architect |
| R-27 | Governance overhead slows a one-person project | Medium | Low | Short checklist, review only at gates | Lead Architect |
| R-10 | Team boundaries T2/T3 create heavy coupling around checkout | Medium | Medium | Define the saga contract in Phase C. Revisit boundaries if the contract is chatty | Lead Architect |

## Assumptions

| ID | Assumption | Validate by |
|---|---|---|
| A-01 | Traffic and order volumes as in A3 section 3.4 | Business plan review, load tests |
| A-02 | A PSP provides tokenisation so card data stays out of scope | PSP selection (Phase D) |
| A-03 | Eventual consistency is acceptable to customers if status is honest | Product owner review (Phase B) |
| A-04 | One cloud region with a recovery site is sufficient. **Confirmed**; design stays multi-region-ready | Phase D |
| A-05 | Baseline is a single monolith with one SQL Server database | **Confirmed by sponsor in Phase B** |
| A-06 | India is the primary market, orders in INR. Tax calculation and GST invoicing are out of scope. **Confirmed** | n/a |
| A-07 | Customers must sign in to pay (no guest checkout). **Confirmed** | n/a |
| A-08 | Retention periods (C1) and saga time limits (C4). **Accepted as reasonable by the sponsor, 2026-10-24** | n/a |
| A-09 | Technology architecture stays cloud-neutral (Q6). **Confirmed** | Phase D |
| A-10 | Per-instance throughput rates, cache hit ratio and limit values in D4 are placeholders | Load tests T-1 to T-3 |
| A-11 | Relative work package sizes S to XL are unmeasured | Actuals after TA-0 and TA-1 |
| A-12 | A simulated payment provider is sufficient for the project | Interface follows real PSP semantics so a sandbox can replace it |
| A-13 | Canary on live traffic is possible through the gateway for every wave | SP-3, WP-05 |
| A-14 | Classic can export stock and order data in a usable form | Paper rehearsal checklist |

## Issues

| ID | Issue | Action | Owner |
|---|---|---|---|
| I-01 | Provisional ADRs (001 to 003) exist before the ADM cycle | Re-validate in Phases B to D. Update status | Lead Architect |
| I-03 | Code spike targets .NET 8, which loses support on 10 Nov 2026 | Retarget to .NET 10 before further code | Lead Architect |
| I-02 | Business figures are role-played, not measured | Mark as assumed everywhere. Replace if real data is available | Lead Architect |

## Dependencies

| ID | Dependency | Needed for |
|---|---|---|
| DEP-01 | Sponsor approval of A2 and A3 | Start of Phase B |
| DEP-02 | Identity provider choice confirmed | Phase D security architecture |
| DEP-03 | PSP contract assumptions (tokenisation, idempotency support) | Payment interface in Phase C |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 0.2 | 2026-10-24 | Lead Architect | Added R-09, R-10, A-06, A-07. A-05 confirmed |
| 0.3 | 2026-10-24 | Lead Architect | Sponsor decisions on Q2 to Q5 recorded: A-04, A-06, A-07 confirmed, R-09 lowered |
| 0.4 | 2026-10-24 | Lead Architect | Phase C: added R-11 to R-13, A-08 |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
| 1.1 | 2026-10-24 | Lead Architect | Phase D: R-14 to R-17, I-03, A-10 added. A-08, A-09 recorded |
| 1.2 | 2026-10-24 | Lead Architect | R-15 reworded and accepted (MassTransit v8). Phase D sponsor decisions recorded |
| 1.3 | 2026-10-24 | Lead Architect | Phase E: R-18 to R-22, A-11, A-12 added |
| 1.4 | 2026-10-24 | Lead Architect | Phase F: R-23 to R-25, A-13, A-14 added |
| 1.5 | 2026-10-24 | Lead Architect | Phase G: R-26, R-27 added |
