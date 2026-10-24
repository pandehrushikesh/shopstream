# B6: Business Gap Analysis, Impact and Phase B Summary

| Field | Value |
|---|---|
| Phase | B |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | B1 to B5, A3, A5 |

## 1. Purpose
Compare the baseline and target business architectures, state what must change and how (new, enhance, adopt, retire), assess impact, and close Phase B with a traceable requirements set and a hand-off to Phases C and D.

## 2. Capability gap analysis

Closure approach: **New** (build), **Enhance** (improve existing), **Adopt** (use a product or service), **Retire**.

| Capability | Baseline situation | Target situation | Gap | Closure | Priority |
|---|---|---|---|---|---|
| CAP-01 Catalogue | Edited in the monolith, no scheduled offers | Offers with start, end, quantity, per-customer limit | Offer scheduling missing | Enhance | Medium |
| CAP-02 Search and browse | Database-driven pages, slow at peak | Cached, scalable reads, search that survives peaks | Performance and scale | Enhance | Medium |
| CAP-03 Basket | Server session memory, needs affinity | Independent, expiring basket | Session coupling | New | Medium |
| CAP-04 Checkout | One long transaction, duplicate-prone | Idempotent, orchestrated, honest status | Process redesign (BP-1 to BP-5) | New | **High** |
| CAP-05 Payment | Synchronous call inside the order transaction | Provider-based, outcome-safe, reconciled | Unknown-outcome handling, reconciliation | Enhance and Adopt | **High** |
| CAP-06 Inventory | A stock column in the order database | Reservation, commit, expiry, limits | Whole capability | New | **High** |
| CAP-07 Order management | Status as a single field, no history | Timeline of states with reasons | Visibility | Enhance | Medium |
| CAP-08 Notifications | Ad hoc emails from the monolith | Event-driven confirmation and cancellation messages | Decoupling | Enhance | Low |
| CAP-09 Identity | Application-managed accounts | Central standards-based identity, roles | Standardisation | Adopt | **High** |
| CAP-10 Edge | Load balancer only | Authentication, rate limiting, routing | Whole capability | Adopt | **High** |
| CAP-11 Observability | Server logs, reactive | Metrics, traces, alerts, business dashboards | Whole capability | Adopt | **High** |
| CAP-12 Delivery platform | Manual deployment | Automated pipeline per service | Whole capability | New and Adopt | **High** |

## 3. Process gaps

| Process | Gap | Closed by |
|---|---|---|
| PRC-02 Place order | BP-1 to BP-5 (B4) | Redesigned flow, rules BR-01 to BR-10, requirements REQ-02, 03, 10, 15 |
| PRC-03 Cancel order | No customer cancellation, no stock/refund link | BR-05, BR-11, REQ-11 |
| PRC-05 Reconcile payments | Manual spreadsheet, after the fact | BR-12, REQ-12 |
| PRC-04 Run flash sale | Informal, no readiness check | VS-5 readiness checklist (B2), REQ-09 |
| PRC-06 Explain an order | Support reads the database | REQ-13 |

## 4. Organisation gaps

| Gap | Target | Notes |
|---|---|---|
| Functional teams around one codebase | Four capability-aligned teams (B3) | Needs training and an agreed contract between T2 and T3 |
| Central DBA gatekeeping | Teams own their data | Governance via principles and review |
| Manual operations | Platform team provides paved path | Prerequisite for O4 and O6 |

## 5. Information gaps

| Gap | Target |
|---|---|
| Single shared schema | One owner per concept (B5, PR-03) |
| No price snapshot on orders | BR-06 |
| No reservation concept | Reservation entity owned by CAP-06 |
| Payment data mixed with order data | Separate restricted class (B5), supports REQ-08 |

## 6. Impact assessment

| Stakeholder | Impact | Level |
|---|---|---|
| Customers | Status shown as "Received" first, then confirmed or cancelled. Fewer failures at peak | Medium (positive, needs clear messaging) |
| Product owners | Must define offers, limits and rules; accept eventual consistency | Medium |
| Support | New order timeline replaces database queries | Medium |
| Finance | Daily reconciliation report | Low (positive) |
| Engineering | New skills, new ownership model | **High** |
| Operations | Moves from servers to a platform, on-call model changes | **High** |
| Security | Central identity, smaller PCI scope | Medium (positive) |

## 7. Requirements and traceability

| Req | Requirement (short) | Objective | Capability | Rule / principle |
|---|---|---|---|---|
| REQ-01 | Browse and search under peak load | O1 | CAP-01, 02, 10 | PR-06 |
| REQ-02 | Idempotent checkout | O3 | CAP-04 | BR-03, BR-04, PR-07 |
| REQ-03 | No oversell | O3 | CAP-06 | BR-01, BR-05 |
| REQ-04 | Visible order status | O2, O6 | CAP-07 | BR-08 |
| REQ-05 | Authenticated and authorised access | O5 | CAP-09, 10 | BR-11, PR-09 |
| REQ-06 | Independent deployment | O4 | CAP-12 | PR-01, PR-10 |
| REQ-07 | Observable system with alerts | O6 | CAP-11 | PR-10 |
| REQ-08 | No card data stored or logged | O5 | CAP-05 | PR-04 |
| REQ-09 | Offers with quantity and per-customer limit | O2, O3 | CAP-01, 06 | BR-07 |
| REQ-10 | Reservations expire | O3 | CAP-06 | BR-01 |
| REQ-11 | Customer cancellation before confirmation | O2 | CAP-07, 04 | BR-05, BR-11 |
| REQ-12 | Daily reconciliation report | O3 | CAP-05 | BR-12 |
| REQ-13 | Order timeline for support | O6 | CAP-07 | BR-08 |
| REQ-14 | Confirmation and cancellation notifications | O2 | CAP-08 | BR-08 |
| REQ-15 | Resolve unknown payment outcomes | O3 | CAP-05 | BR-10 |

Every objective O1 to O6 is covered by at least one requirement. Checking this: O1 (REQ-01), O2 (REQ-04, 09, 11, 14), O3 (REQ-02, 03, 09, 10, 12, 15), O4 (REQ-06), O5 (REQ-05, 08), O6 (REQ-04, 07, 13).

## 8. Changes recorded in this phase

| ID | Change | Affects |
|---|---|---|
| C-01 | Flash sales are handled as offers inside CAP-01 and CAP-06. A promotions and pricing engine stays out of scope | A3 scope, B1 |
| C-02 | Requirements REQ-09 to REQ-15 added | A3 list extended |
| C-03 | Q1 resolved: the monolith baseline is confirmed by the sponsor | A3, A5 (A-05) |
| C-04 | Q2: India is the primary market, orders in INR (confirmed). Tax calculation and GST invoicing remain out of scope | A5 (A-06) |
| C-05 | Q3, Q4, Q5 resolved by the sponsor (see Open points). BR-01, BR-06, BR-07, BR-09 are now Confirmed | B3, B4, A5 |

## 9. Risks and open points raised

New entries are in A5: R-09 (business rules unvalidated), R-10 (team boundaries vs coupling), A-06, A-07.

| # | Open question | Needed by |
|---|---|---|
| Q3 | One region with recovery site, or multi-region? | **Resolved 2026-10-24: one region plus recovery site; design must stay multi-region-ready (REQ-21 in C5)** |
| Q4 | Is guest checkout required, or must customers sign in to pay? | **Resolved 2026-10-24: sign-in required to pay** |
| Q5 | Confirm BR-01 (10-minute reservation), BR-06 (price snapshot), BR-07 (per-customer limit) | **Resolved 2026-10-24: confirmed** |

## 10. Hand-off to Phases C and D

| To | Content |
|---|---|
| Phase C Data | Concepts and owners (B5), sensitivity classes, price snapshot and reservation rules |
| Phase C Application | Capability priority (B1), candidate service boundaries from capability ownership, saga steps and exception paths (B4), interface needs between T2 and T3 |
| Phase D Technology | Objectives O1 to O6, peak scenario VS-5, security needs from B3 role matrix, observability metrics (B2), delivery needs (CAP-12) |

Candidate service boundaries to test in Phase C (not yet decisions): Catalogue, Basket, Ordering, Inventory, Payment, Notification, plus platform components for identity and edge. Phase C must also test the PR-02 alternative: a modular monolith with the same boundaries.

## 11. Phase B exit criteria

| Criterion | Status |
|---|---|
| Baseline and target capability models agreed (B1) | Met (approved 2026-10-24) |
| Value streams mapped to capabilities (B2) | Met (approved 2026-10-24) |
| Organisation and roles defined (B3) | Met (approved 2026-10-24) |
| Critical process and rules modelled (B4) | Met (approved 2026-10-24) |
| Business information concepts and owners defined (B5) | Met (approved 2026-10-24) |
| Gaps, impacts and requirements traced to objectives (B6) | Met (approved 2026-10-24) |
| Open questions listed, none blocking Phase C | Met |

## 12. Decisions requested
1. Approve B1 to B6 as the Business Architecture baseline and target.
2. Answer Q4 and confirm the business rules in Q5.
3. Approve moving to Phase C.

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 0.2 | 2026-10-24 | Lead Architect | Q3, Q4, Q5 resolved, C-05 added |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
