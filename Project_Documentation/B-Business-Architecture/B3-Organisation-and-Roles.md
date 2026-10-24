# B3: Organisation, Actors and Roles

| Field | Value |
|---|---|
| Phase | B |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | A4, B1, B2 |

> Fictional case study. Structures are `[assumed]`.

## 1. Purpose
Describe who does the work today, who must do it in the target, and where responsibilities change. Architecture and organisation shape each other (Conway's law): services owned by no one, or owned by too many, fail.

## 2. Baseline organisation (`[assumed]`)

| Team | Responsibility | Pain |
|---|---|---|
| Web development | Builds and changes the whole monolith | Any change risks every capability. Releases are batched |
| Database administration | Owns the single database, schema changes, tuning | Bottleneck for every schema change |
| QA | Manual regression before each release | Slow, late feedback |
| Operations | Manual deployment, server upkeep, reacts to alarms | No visibility, long recovery |

## 3. Target organisation

Four teams aligned to capabilities, following stream-aligned and platform team patterns. Each team owns its services end to end: design, build, test, deploy, run.

| Team | Type | Owns | Capabilities |
|---|---|---|---|
| T1 Discovery | Stream-aligned | Catalogue, search and browse | CAP-01, CAP-02 |
| T2 Checkout and Orders | Stream-aligned | Basket, checkout, orders, notifications | CAP-03, CAP-04, CAP-07, CAP-08 |
| T3 Inventory and Payments | Stream-aligned | Stock reservation and payment orchestration | CAP-05, CAP-06 |
| T4 Platform | Platform | Identity, edge, observability, delivery pipeline | CAP-09, CAP-10, CAP-11, CAP-12 |

Supporting roles outside the teams: Lead Architect (principles, reviews), Security and compliance officer (reviews, not a gate on every change), Finance (reconciliation consumer).

Design note: T2 and T3 interact heavily during checkout. That is deliberate: their boundary is the saga contract, which Phase C will define precisely. If the contract proves too chatty, the boundary is revisited (see R-04, R-01).

## 4. Business actors and roles

| Actor | Role | Main interactions |
|---|---|---|
| Customer | Shopper | Browse, basket, checkout, track orders |
| Guest | Anonymous shopper | Browse, basket. **Sign-in is required to pay (confirmed)** |
| Merchandiser | Catalogue editor | Maintain products and categories |
| Campaign manager | Offer planner | Define and schedule flash-sale offers |
| Support agent | Customer support | View order timeline, explain failures, request cancellation |
| Finance analyst | Reconciler | Review payment-to-order reconciliation |
| Platform engineer | Builds and runs the platform | Deploy, configure, maintain tooling |
| SRE on call | Responds to incidents | Monitor, triage, restore |
| Security officer | Assurance | Review controls and access |
| PSP (external) | Payment provider | Authorise and capture payments |

## 5. Role to access need (input to authorisation design in Phase C/D)

| Role | Read catalogue | Edit catalogue | Create offers | Place orders | View any order | Cancel any order | Reconciliation | Platform admin |
|---|---|---|---|---|---|---|---|---|
| Guest | x | | | | | | | |
| Customer | x | | | own | own | own (before confirm) | | |
| Merchandiser | x | x | | | | | | |
| Campaign manager | x | | x | | | | | |
| Support agent | x | | | | x | request | | |
| Finance analyst | | | | | x | | x | |
| Platform engineer | | | | | | | | x |
| SRE on call | | | | | x (diagnostic) | | | x (limited) |

## 6. Responsibility matrix for key processes (R responsible, A accountable, C consulted, I informed)

| Process | T1 | T2 | T3 | T4 | Product owner | Security | Finance |
|---|---|---|---|---|---|---|---|
| Publish a product | R | I | | | A | | |
| Run a flash sale (go-live readiness) | C | R | R | R | A | C | I |
| Process an order | | R | R | C | A | | I |
| Handle payment failure | | C | R | | A | | I |
| Respond to incident | C | C | C | R | I | C | |
| Change a public contract | C | R | C | C | A | C | |

## 7. People and skill impacts

| Area | Change | Mitigation |
|---|---|---|
| Distributed-systems skills | Sagas, messaging, idempotency | Training and pairing. Start with the checkout flow only |
| Operating many services | On-call and observability practice | T4 provides paved path and templates |
| Ownership | Teams own run as well as build | Shared runbooks, incident reviews |
| DBA role | Shifts from central gatekeeper to platform advice | Teams own their schemas |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
