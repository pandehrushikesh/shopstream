# F1: Migration Strategy and Wave Plan

| Field | Value |
|---|---|
| Phase | F |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | A3, B6, C5 section 5 and GA gaps, D1 to D6, E1 (work packages, transition architectures), E2 |

## 1. Purpose and scope

Phase F decides **how** "ShopStream Classic" (the fictional monolith: two VMs, one SQL Server database, in-memory sessions) is replaced by the target services without stopping trading. Per the sponsor decision in E1, Classic is not built as code. This document is the plan we would follow, and WP-16 rehearses it on paper.

The new services themselves are real and tested. What is fictional is the legacy side, so each wave carries a "what we would do against Classic" column.

## 2. Migration principles

| # | Principle | Consequence |
|---|---|---|
| M-1 | Strangler at the gateway | The YARP gateway routes each capability to Classic or to the new service. Routes switch per capability, never all at once |
| M-2 | One owner of each fact at all times | During a wave, data has exactly one writer (Classic or the new service). No two-way sync of the same data |
| M-3 | Reversible until proven | Every switch has a rollback path and a decision point. Data written by the new side must be returnable or replayable |
| M-4 | Small, dark first | New code runs unseen (shadow traffic), then for a small share (canary), then fully |
| M-5 | Never migrate during a business peak | Change freeze rules (section 6) |
| M-6 | Customers do not notice | No forced password reset where avoidable, no lost baskets or orders |

## 3. Cut-over pattern

Each wave follows the same stages, with an explicit exit test for each.

| Stage | What happens | Exit test |
|---|---|---|
| 1. Build and verify | New service passes its tests in staging (and T-tests relevant to the wave) | Tests green |
| 2. Shadow | Gateway copies read traffic to the new service. Responses are compared, never shown | Differences within agreed tolerance for 3 days |
| 3. Canary | 1%, then 10%, then 50% of live traffic served by the new service (Tier 1: canary per D5) | Error rate and p95 within SLO at each step, no business-rule alerts |
| 4. Full cut-over | 100% routed to the new service. Classic route kept warm | 7 days stable |
| 5. Decommission the old path | Classic code path and data for this capability switched off | Sponsor sign-off and backup retained per retention rules |

Rollback: change the gateway route back. For waves that move writes (3), rollback uses the data rules in section 4.

## 4. Wave plan

| Wave | Content | Transition architecture | Data approach | Rollback |
|---|---|---|---|---|
| 0 | Platform: cluster, pipeline, observability, identity, gateway | TA-0, TA-1 | Accounts: import customer records into Keycloak. Passwords: migrate hashes with a "verify on first sign-in, re-hash to Keycloak policy" step so customers do not reset (M-6). Fall back to reset email for accounts that cannot be migrated | Gateway continues sending sign-in to Classic until accounts are imported and verified |
| 1 | Catalogue (read only) | TA-2 | One-way copy from the Classic SQL Server product tables into MongoDB, repeated until cut-over. Classic remains the writer of catalogue changes until the new Catalogue admin goes live, then the new service becomes the only writer and the copy stops | Route reads back to Classic. Re-run the copy |
| 2 | Basket | TA-3 | Baskets are short-lived (expiry in C1). No data migration: empty baskets at switch, with a notice. Switch at a quiet hour | Route back. Customers rebuild baskets |
| 3 | Inventory, Payment, Ordering together | TA-4 | **Drain and switch.** Stock levels copied to Inventory in a freeze window. New orders go to the new saga, orders in progress in Classic finish in Classic. Order history copied read-only to Ordering for the support timeline. Payment ledger starts empty and old payments stay in Classic for reconciliation | See section 5 |
| 4 | Notification, then retire Classic | TA-5 | Notifications switch to event consumers. Classic is switched off after one full reconciliation cycle with no differences | Re-enable Classic route until the decommission date |

## 5. Wave 3 in detail (the risky one)

Wave 3 moves money and stock, so it gets extra controls.

| Topic | Rule |
|---|---|
| Single stock owner | Stock for a product is owned by Classic until its **switch time** and by Inventory afterwards (M-2). Switch per product group, not globally |
| Order cut-off | Orders created before the switch finish where they started. New orders after the switch use the new saga |
| In-flight payments | No payment is moved. Unknown outcomes are resolved by the system that started them (BR-12) |
| Reconciliation | Daily comparison of stock, orders and payments across both systems during the wave, with a zero-difference target before the next product group moves |
| Rollback within hours | Pause new-saga routing, let in-flight new orders finish, return stock ownership to Classic with a final stock export, then re-route. This is rehearsed in staging first (T-5, T-7) |
| Rollback after days | Not a technical rollback: forward-fix, because the new side holds the truth. Accepted risk R-23 |
| Customer promises | No oversell and no duplicate charge hold during the switch (O3). A flash sale is never the first live use of the new saga |

## 6. Change freeze and timing rules

| Rule | Detail |
|---|---|
| Never during a trading peak | No cut-over stage within 48 hours before or during a planned flash sale or seasonal peak |
| Quiet windows | Cut-over steps at low-traffic hours in the primary market (India night) |
| Freeze on the other side | While a capability is in canary or full cut-over, no schema or logic changes to that capability in Classic |
| Hold points | Waves 0 to 2 may run in parallel with development. Wave 3 requires T-1 to T-8 and a recovery drill (T-7) to have passed first |
| Emergency change | Allowed only to fix a live incident, recorded as an ADR note afterwards |

## 7. Coexistence design

| Concern | Approach |
|---|---|
| Identity | One identity provider (Keycloak) from wave 0. Classic trusts Keycloak tokens through a small adapter during coexistence |
| Shared database | The new services never read Classic tables directly. They use copies, APIs or events (PR-03) |
| Events | Classic publishes order and stock changes to RabbitMQ through a change-data-capture style adapter while coexisting. This is documented only, as Classic is fictional |
| Observability | Same trace and correlation ID across gateway, Classic and new services so a request can be followed across both worlds |
| Support | One timeline view of an order even if its history is partly in Classic (read-only copy) |

## 8. Paper rehearsal of Classic retirement (WP-16)

The rehearsal is a table-top exercise with a written script:

1. List every Classic function and confirm its new owner (traceable to B1 capabilities).
2. For each wave, walk through the five stages with an assumed incident (for example, shadow differences above tolerance, or a payment unknown-outcome spike) and write down the decision and rollback.
3. Confirm retention: which Classic data must be archived, for how long (C1 periods) and where.
4. Record the result as an annex to F2 with "decision logs" for at least three simulated incidents.

## 9. Risks and assumptions

| ID | Type | Statement | Response |
|---|---|---|---|
| R-23 | Risk | After days of live trading in wave 3, rolling back is no longer possible, only fixing forward | Staged product groups, daily reconciliation, rehearsed rollback in the first hours |
| R-24 | Risk | Password hash formats in Classic cannot be migrated | Fallback reset email, communication plan |
| R-25 | Risk | Shadow comparison produces noise that hides real defects | Agree tolerances in advance, ignore known non-determinism (timestamps, ordering) |
| A-13 | Assumption | A canary on live traffic is possible through the gateway for every wave | Confirm in SP-3 and WP-05 |
| A-14 | Assumption | Classic can export stock and order data in a usable form | Part of the paper rehearsal checklist |

## 10. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve migration principles M-1 to M-6 (section 2) | **Approved** |
| 2 | Approve the five-stage cut-over pattern (section 3) | **Approved** |
| 3 | Approve the wave plan and the drain-and-switch approach for wave 3 (sections 4, 5) | **Approved** |
| 4 | Approve freeze and timing rules (section 6) | **Approved** |
| 5 | Approve the paper rehearsal as the way to cover WP-16 (section 8) | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
