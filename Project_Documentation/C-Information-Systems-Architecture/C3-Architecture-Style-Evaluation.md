# C3: Architecture Style Evaluation (monolith, modular monolith, services)

| Field | Value |
|---|---|
| Phase | C (Application Architecture) |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | A3 objectives O1 to O6, B1 to B6, P1 (PR-02) |
| Decision record | `docs/adr/004-architecture-style.md` |

> Fictional case study. Scores are the architect's judgement, made visible so they can be challenged.

## 1. Purpose
PR-02 says complexity must earn its place and requires the alternatives to microservices to be considered and rejected on evidence. This document does that, and states plainly how strong the evidence is.

## 2. Options

| Option | Description |
|---|---|
| **A. Improved monolith** | Keep one deployable. Add a cache, a background job queue, more instances, read replicas, and automate deployment |
| **B. Modular monolith + extracted hot services** | One deployable core with strict module boundaries (catalogue, basket, orders, notifications). Inventory and Payment extracted as separate services. Message broker for the order flow |
| **C. Capability-aligned services** | Independently deployable services per capability (Catalogue, Basket, Ordering, Inventory, Payment, Notification) behind an API gateway, each owning its data, with a message broker |

## 3. Criteria and weights

Criteria come directly from the objectives, plus PR-02.

| Criterion | Objective | Weight (base case) |
|---|---|---|
| Hot-path scalability | O1 | 20 |
| Transactional integrity under contention and retries | O2, O3 | 25 |
| Independent delivery by four teams | O4 | 20 |
| Security and isolation | O5 | 10 |
| Operability and recovery | O6 | 10 |
| Simplicity and cost to run | PR-02 | 15 |

## 4. Scores (1 = poor, 5 = strong) and reasons

| Criterion | A | B | C | Reasoning |
|---|---|---|---|---|
| Hot-path scalability | 3 | 4 | 5 | A scales the whole app, even rarely-hit parts. B scales the checkout path separately. C scales each capability on its own load profile (reads 95% vs orders 5%) |
| Transactional integrity | 4 | 4 | 3 | A has local transactions but the stock row is a contention point. B keeps order and basket local and isolates stock and payment. C pays a consistency price: sagas, idempotency, eventual consistency |
| Independent delivery | 1 | 3 | 5 | A is one release train. B has one shared core release plus two services. C lets four teams release separately |
| Security and isolation | 2 | 3 | 4 | Smaller blast radius and a payment boundary reduce PCI scope in B and C |
| Operability and recovery | 3 | 3 | 4 | A is simple but fails as a whole. C contains failures but needs mature observability |
| Simplicity and cost to run | 5 | 3 | 2 | Operational burden grows from A to C (PR-02) |

## 5. Weighted results

| Scenario | A | B | C |
|---|---|---|---|
| Base case (weights above) | 3.05 | 3.45 | **3.85** |
| Sensitivity 1: independent delivery 10, simplicity 25 | 3.45 | 3.45 | **3.55** |
| Sensitivity 2: integrity 35, simplicity 15, delivery 10 | 3.35 | 3.55 | **3.65** |

Weights in sensitivity 1 are 20/25/10/10/10/25, in sensitivity 2 they are 20/35/10/10/10/15 (same order as section 3).

**Reading the result honestly:**
- C leads in every scenario, but in the two sensitivity cases the margin is small and B is a credible, cheaper alternative.
- The result depends mainly on how much weight O4 (independent delivery by four teams) carries. If the organisation stayed at one team, B would win.
- Option A fails O4 outright and is the weakest on O1, so it is rejected.

## 6. Context disclosure
ShopStream is also a deliberate learning and portfolio project. Exercising service decomposition, sagas, and a gateway is an explicit secondary goal. That goal was **not** scored as a criterion, and the table shows it is not needed for C to lead. It is stated here so that readers can judge the decision with full information.

## 7. Decision
**Adopt Option C, capability-aligned services, with guardrails**, recorded as ADR-004:

1. Start with six domain services (Catalogue, Basket, Ordering, Inventory, Payment, Notification) plus platform components. Do not split further without evidence.
2. **Merge trigger:** if two services must change together in more than half of their releases over a quarter, or share a transaction need, merge them.
3. **Fallback:** if platform maturity (CAP-11, CAP-12) is not reached by the first production release, deploy Ordering, Basket and Notification as one deployable (Option B) while keeping module boundaries and contracts identical. The contracts in C4 make this possible.
4. Every service must name an owning team (B3) and a capability (B1).

## 8. What would change the decision
- Team count falls to one or two.
- Measured load (Phase D tests) shows reads and orders can be served comfortably by one scaled deployable.
- Platform capabilities cannot be staffed.

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
