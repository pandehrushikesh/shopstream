# B1: Business Capability Model

| Field | Value |
|---|---|
| Phase | B |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | A3 (capability map, objectives), P1 (PR-01, PR-11) |

> Fictional case study. Maturity scores and classifications are the architect's `[assumed]` judgement.

## 1. Purpose
Decompose the Level-1 capabilities from A3 into Level-2 capabilities, state baseline and target maturity, classify each by strategic value, and assign ownership. This is the stable backbone for service boundaries in Phase C (PR-01).

## 2. Maturity scale

| Level | Meaning |
|---|---|
| 1 | Ad hoc: manual, person-dependent |
| 2 | Repeatable: works in normal conditions, breaks under stress |
| 3 | Defined: documented, consistent |
| 4 | Managed: measured against objectives, automated |
| 5 | Optimised: resilient under peak, continuously improved |

## 3. Capability catalogue

Strategic class: **Differentiating** (build, competitive advantage), **Core** (needed, build or configure), **Commodity** (adopt or buy, PR-11).

| ID | Capability | Level-2 capabilities | Class | Baseline | Target | Objectives |
|---|---|---|---|---|---|---|
| CAP-01 | Catalogue management | Product maintenance, category management, attribute management, **offer scheduling** (time-boxed limited-stock offers) | Core | 3 | 4 | O1 |
| CAP-02 | Search and browse | Listing and filtering, keyword search, product detail view | Core | 2 | 4 | O1 |
| CAP-03 | Basket | Add/remove items, basket expiry, price display | Core | 2 | 4 | O1 |
| CAP-04 | Checkout and ordering | Checkout submission, duplicate-submission protection, order placement, order orchestration | **Differentiating** | 2 | 5 | O2, O3 |
| CAP-05 | Payment | Payment authorisation and capture, failure handling, refund/void, reconciliation | Commodity (provider) with Core orchestration | 2 | 4 | O3, O5 |
| CAP-06 | Inventory and reservation | Stock keeping, reservation and release, stock commit, per-customer limits | **Differentiating** | 1 | 5 | O3 |
| CAP-07 | Order management | Order status tracking, cancellation, order history, support timeline | Core | 2 | 4 | O2 |
| CAP-08 | Notifications | Order confirmation and cancellation messages, delivery tracking | Commodity | 2 | 3 | O6 |
| CAP-09 | Identity and access | Registration and sign-in, roles and permissions, service-to-service trust | Commodity | 2 | 4 | O5 |
| CAP-10 | Edge and traffic management | Routing, rate limiting, request protection, API exposure | Commodity | 1 | 4 | O1, O5 |
| CAP-11 | Observability | Logging, metrics, tracing, alerting, business dashboards | Commodity | 1 | 4 | O6 |
| CAP-12 | Delivery platform | Build and test automation, deployment, environments, configuration and secrets | Commodity | 1 | 4 | O4 |

Why the classes matter: the differentiating capabilities (CAP-04, CAP-06) are where flash-sale reliability is won or lost, so they receive the deepest design in Phase C. Commodity capabilities should be adopted rather than built (PR-11).

## 4. Capability heat map (gap size = target minus baseline)

| Gap | Capabilities |
|---|---|
| 4 | CAP-06 Inventory and reservation |
| 3 | CAP-04 Checkout and ordering, CAP-10 Edge, CAP-11 Observability, CAP-12 Delivery platform |
| 2 | CAP-02, CAP-03, CAP-05, CAP-07, CAP-09 |
| 1 | CAP-01, CAP-08 |

Priority for Phase C, ordered by gap and objective weight: CAP-06, CAP-04, CAP-05, CAP-09/10, CAP-11/12, then the rest.

## 5. Capability ownership (target)

| Capability | Accountable business owner | Delivering team (see B3) |
|---|---|---|
| CAP-01, CAP-02 | Product owner, Discovery | T1 Discovery |
| CAP-03, CAP-04, CAP-07, CAP-08 | Product owner, Checkout | T2 Checkout and Orders |
| CAP-05, CAP-06 | Product owner, Fulfilment and Payments | T3 Inventory and Payments |
| CAP-09, CAP-10, CAP-11, CAP-12 | Operations / SRE lead | T4 Platform |

## 6. Scope refinement recorded in Phase B
A3 left campaign handling implicit. Phase B resolves it: **flash sales are handled as time-boxed, limited-stock offers** inside CAP-01 (offer scheduling) and CAP-06 (allocation and per-customer limits). A promotions and pricing engine remains out of scope. See B6, change C-01.

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
