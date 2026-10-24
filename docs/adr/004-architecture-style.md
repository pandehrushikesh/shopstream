# ADR-004: Capability-aligned services (with guardrails)

- **Status:** Accepted (Phase C), reviewable on the triggers below
- **Date:** 2026-10-24
- **Full evaluation:** `Project_Documentation/C-Information-Systems-Architecture/C3-Architecture-Style-Evaluation.md`

## Context
Objectives O1 to O6 (A3) must be met by four teams. Three styles were compared: improved monolith (A), modular monolith with extracted Inventory and Payment (B), capability-aligned services (C).

## Decision
Adopt Option C. Six domain services (Catalogue, Basket, Ordering, Inventory, Payment, Notification) plus platform components (gateway, identity, broker, observability). Each service is owned by one team and one capability, and owns its data.

## Consequences
- (+) Independent delivery (O4) and per-capability scaling (O1). Failures are contained. Payment is an isolated boundary (O5).
- (-) Distributed consistency must be engineered (saga, idempotency, outbox). Higher operational cost.
- Evidence margin is narrow under some weightings; Option B stays a documented fallback.

## Guardrails and review triggers
1. No further splitting without evidence.
2. Merge services that change together in more than half of releases over a quarter.
3. If platform maturity is not reached for the first release, deploy Ordering, Basket and Notification as one unit with unchanged contracts.
4. Re-evaluate if team count drops, or load tests show one deployable suffices.

Learning value was a secondary goal and was not scored.
