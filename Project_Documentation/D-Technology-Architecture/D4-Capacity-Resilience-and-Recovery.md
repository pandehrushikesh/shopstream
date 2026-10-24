# D4: Capacity, Resilience, Rate Limiting and Recovery

| Field | Value |
|---|---|
| Phase | D |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | A3 (section 3.4 numbers, O1 to O6), C1 section 7, C2 sections 6 and 7, C4, D1, D3, sponsor decision Q3 |

> All per-instance throughput figures are **planning assumptions** (marked `[assumed]`) to be replaced by measurements from the load tests in section 8 (risk R-03).

## 1. Purpose
Turn the performance and availability objectives into a sized, defensible design: how many instances, how limits protect the system, how failures are absorbed, how recovery works, and how we will prove it.

## 2. Capacity model

### 2.1 Demand (from A3 and C2)

| Quantity | Value | Source |
|---|---|---|
| Edge peak | 3,000 req/s | A3 |
| Catalogue reads at peak | about 2,850 req/s (95%) | A3 |
| Order attempts, burst | 500/s | O2 |
| Assumed burst duration | 5 minutes, so about 150,000 attempts `[assumed]` | "several minutes" in A3 |
| Messages per order | 6 published, about 8 delivered (fan-out to Notification, Payment, Basket) | C4 |
| Message burst | 3,000 published/s, about 4,000 delivered/s | 500 x 6 and 500 x 8 |
| Ordering database transactions | about 4 per order, so about 2,000 tx/s at burst (upper bound) | placement plus saga steps |

### 2.2 Sizing rule
Instances = demand / per-instance rate, then enough extra that **losing one of three zones still leaves the peak served**, spread evenly across zones (multiples of 3). Minimum 3 instances for Tier 1 and Tier 2 (one per zone).

| Service | Per-instance rate `[assumed]` | Demand | Instances for peak | With one zone lost | Planned at peak |
|---|---|---|---|---|---|
| Gateway | 1,000 req/s (2 vCPU) | 3,000 req/s | 3 | 3 / (2/3) = 4.5 | **6** |
| Catalogue (cached) | 800 req/s | 2,850 req/s | 4 | 4 / (2/3) = 6 | **6** |
| Ordering | 150 placements/s | 500/s | 4 | 6 | **6** |
| Inventory | 200 reservations/s | 500/s | 3 | 4.5 | **6** |
| Payment | 100 payments/s (provider-bound) | at most the reserved units, see 2.3 | 1 | 1.5 | **3** (minimum) |
| Notification | 200/s | 500/s | 3 | 4.5 | **6** |
| Basket | Proportional to browse sessions | not on the burst path beyond checkout read | 3 | n/a | **3** (minimum) |

Average-day load (about 350 req/s) needs only the minimum of 3 instances per service. Peak counts are reached by **pre-scaling before planned flash sales** and autoscaling for unplanned peaks.

### 2.3 The reservation step protects the payment provider
Most flash-sale attempts exceed available stock. Those attempts are rejected at the **reservation** step (cheap) and never reach Payment. Payment demand is therefore bounded by the **units reserved**, not by the attempt rate.

Rule for offers: units sold per minute must not exceed the payment provider's sustained rate. Example: a provider rate of 100 authorisations/s `[assumed]` allows 6,000 units per minute. A 5,000-unit offer drains in about 50 seconds, well inside the 10-minute reservation (BR-01). A provider limit below this would make reservations expire before payment, so the provider's limit is a dependency (DEP-03) that sets maximum offer size.

### 2.4 Data stores

| Store | Design | Sizing concern |
|---|---|---|
| PostgreSQL (Ordering) | Primary plus synchronous replica in another zone, asynchronous replica in the recovery region | About 2,000 tx/s at burst. Plan 16 vCPU with fast storage and test at 1.5x (3,000 tx/s) |
| PostgreSQL (Inventory, Payment) | Same pattern, separate instances | Hot-row updates on Inventory (C1 section 7) |
| MongoDB (Catalogue) | Three-member replica set across zones, one extra member in the recovery region | About 285 reads/s after 90% cache hit `[assumed]`, small |
| Redis-compatible | Primary plus replica across zones | Basket and sessions about 100k active x 2 KB, about 200 MB each, small |
| RabbitMQ | Three nodes across zones, quorum queues, separate queues per consumer | 3,000 to 4,000 msg/s total. Planning limit about 2,000 to 5,000 msg/s per queue with confirms `[assumed]`. Queues per consumer keep each below 1,500 msg/s |

If measured broker throughput falls short, options in order: add queue partitions per hot consumer, move the highest-volume stream to a log-based broker (ADR-007 scale-out), reduce message count per order.

## 3. Resilience policies

Applied with the standard .NET resilience library (TS-14). Values `[assumed]`, tuned by tests.

| Call | Timeout | Retries | Circuit breaker | Fallback / behaviour |
|---|---|---|---|---|
| Gateway to any service | 2 s reads, 5 s writes | None for writes. 1 for idempotent reads | Per-service | 503 with Retry-After |
| Ordering to Basket (IF-04) | 300 ms | 1 with jitter | Yes | Reject checkout before payment |
| Service to PostgreSQL | 2 s command timeout | Transient errors only, 2 attempts | n/a | Fail request. Health shows not ready |
| Payment to provider (charge) | 8 s | At most 2, **only with the same idempotency reference** | Opens at 50% failures over 30 s | Unknown outcome path (BR-10) |
| Payment to provider (status query) | 3 s | 3 with backoff | Yes | Keep order in resolving state, alert after 10 minutes |
| Message consumers | n/a | 5 attempts, exponential backoff with jitter | n/a | Then dead-letter queue, alert, replay tool |
| Publishing | Outbox relay retries until acknowledged | Unbounded, with backoff | n/a | Alert if outbox age exceeds 1 minute |

Bulkheads: separate connection pools and thread budgets per dependency, and a separate gateway node pool (D1) so one slow dependency cannot exhaust everything.

Health model: every service exposes liveness (process is up) and readiness (dependencies it needs to serve are reachable), per REQ-24. Liveness never checks dependencies.

## 4. Rate limiting and admission control

### 4.1 Design
- Implemented in the **gateway** (CAP-10). The framework's built-in limiters keep counters per instance only, so the per-IP, per-user and global limits that must hold across instances use **shared counters in the Redis-compatible store**, updated atomically.
- Algorithms: token bucket for steady-with-burst limits, sliding window for login, concurrency limit for checkout in-flight requests.
- Rejected requests get HTTP 429 with a Retry-After header and a problem-details body (REQ-25).
- If the shared store is unavailable, each gateway instance falls back to **local limits set to the global limit divided by the number of instances**. The system fails safe, not open.

### 4.2 Limit table `[assumed]`

| Class | Key | Algorithm | Limit | Burst |
|---|---|---|---|---|
| Anonymous browse | IP address | Token bucket | 20 req/s | 60 |
| Authenticated API | User id | Token bucket | 10 req/s | 30 |
| Login and token routes | IP and username | Sliding window | 5 per minute | n/a |
| Place order | User id | Concurrency limit 1 in flight, plus token bucket | 1 per 2 s | 3 |
| **Checkout admission (global)** | Whole platform | Token bucket, shared | 600 orders/s (500 target plus 20% headroom) | 100 |
| Staff and admin APIs | User id | Token bucket | 5 req/s | 15 |

Notes:
- Per-IP limits must be generous because mobile carriers and offices share addresses. After sign-in, per-user limits take over.
- The global admission limit protects Ordering and its database. Above it, customers receive a polite "high demand, please retry" response **before** any state is created, which is cheaper and safer than accepting and failing later.

## 5. Caching

| Layer | What | Freshness |
|---|---|---|
| CDN | Static assets. Public catalogue GET responses for anonymous users | 30 to 60 seconds, with event-driven purge on price or offer change |
| Gateway or service cache | Product detail and lists | Same TTL, invalidated by Catalogue events |
| Ordering price list | Replica built from Catalogue events (C4 DD-1) | Eventual, bounded lag, resynchronised periodically |

**DD-5 (new design decision): price-change confirmation.** Because displayed prices can be up to a minute old, the place-order request carries the price the customer saw. If Ordering's current price differs, it rejects with "price changed" and shows the new price for confirmation, so nobody is charged an amount they did not see. This complements BR-06 (the price is fixed once the order is received).

Offers starting at a set time: caches for the offer's products are warmed and TTLs shortened around start time (part of the VS-5 readiness checklist).

## 6. Disaster recovery (one region plus recovery site, Q3)

### 6.1 Targets
O6: RTO 30 minutes, RPO 1 minute for orders. Zone failure within the primary region must cause no data loss and no outage beyond a brief failover.

### 6.2 Approach: warm standby
Recovery region holds replicated data, deployed (scaled-down) services, and tested automation. Traffic switches on a failover decision.

| Store | Zone failure | Region failure (RPO) | Recovery step |
|---|---|---|---|
| PostgreSQL | Synchronous replica takes over, no loss | Asynchronous replica plus archived logs. **Alert if replication lag exceeds 30 s** so RPO stays under 1 minute | Promote replica |
| MongoDB | Replica set elects new primary | Recovery member, catalogue loss up to a few minutes is acceptable and republishable | Promote member |
| Redis-compatible | Replica takes over | Not replicated across regions. Baskets and sessions are lost (accepted in C1) | Start empty. Customers sign in again and rebuild baskets |
| RabbitMQ | Quorum queues survive a node loss | In-flight messages are lost | Recovered by design, see 6.3 |
| Keycloak | Replicas across zones | Database replicated. Realm configuration kept in Git | Start in recovery region |
| Secrets, images, manifests | Replicated registry and secret manager, Git | n/a | Already present |

### 6.3 Why lost broker messages are survivable
- State changes and their events are saved together in the database (outbox), so any event not yet delivered is still in the database and is re-sent after recovery.
- Consumers ignore duplicates (inbox), so re-sending is safe.
- The Ordering saga has time limits, so orders stuck because a message was lost are retried or cancelled with compensation.
- Unknown payment outcomes are resolved by asking the provider (BR-10).

Open risk: messages already consumed by a service and acknowledged but whose effects were in a database transaction lost to replication lag. The replication-lag alert and the saga reconciliation (BR-12) cover this; the failover drill must test it explicitly.

### 6.4 Failover runbook outline and time budget

| Step | Budget (min) |
|---|---|
| 1. Declare disaster, freeze writes in the primary | 5 |
| 2. Promote data stores in the recovery region | 5 |
| 3. Scale services and verify broker and identity | 10 |
| 4. Switch traffic (low-TTL DNS or global load balancer) | 5 |
| 5. Smoke tests, reconciliation check, announce | 5 |
| **Total** | **30** |

The budget has **no slack**. Mitigations: keep the recovery site at about one third of peak capacity so step 3 is a scale-up, not a cold start; automate steps 2 to 4; rehearse twice a year (REQ-31). If drills show more than 30 minutes, either the target or the design changes, through change control.

### 6.5 Multi-region readiness
Matches C1 section 12. Staying within India keeps data residency. Moving to active-active would additionally need conflict-free ids (already global), region-aware routing, and partition of stock ownership per region, all deferred.

## 7. Failure scenarios the design must survive

| ID | Scenario | Expected outcome |
|---|---|---|
| F-1 | One pod or node dies | No customer impact |
| F-2 | One zone lost | Degraded capacity only, peak still served (section 2.2) |
| F-3 | Primary database fails | Failover with no order loss. Orders in progress retry |
| F-4 | Payment provider slow (5 s) or down | Breaker opens, orders wait or cancel without double charge |
| F-5 | Broker node lost | Queues continue (quorum). No lost messages |
| F-6 | Redis-compatible store down | Baskets unavailable (checkout rejected before payment), rate limiting falls back to local limits |
| F-7 | Catalogue down | Cached browse continues. Checkout unaffected |
| F-8 | Traffic spike above 600 orders/s | Excess rejected at admission with 429, backend stays healthy |
| F-9 | Consumer crash during processing | Message redelivered, handled once in effect |
| F-10 | Region lost | Recovery within RTO 30 / RPO 1 |

## 8. Verification plan (staging, production-shaped)

| Test | Load / fault | Pass criteria | Objective |
|---|---|---|---|
| T-1 Baseline | 350 req/s mixed, 1 hour | Errors below 0.1%, p95 catalogue read at most 200 ms | O1 |
| T-2 Peak | Ramp to 3,000 req/s, 30 minutes | Same criteria | O1 |
| T-3 Flash sale | 500 orders/s for 5 minutes on one hot product with limited stock | Zero oversold units, zero duplicate charges, every accepted order terminal, backlog drained within 5 minutes after the burst | O2, O3 |
| T-4 Admission | 900 orders/s attempts | Excess answered 429 quickly, accepted flow unaffected | O2 |
| T-5 Failure injection | Run F-1 to F-9 during T-2 and T-3 | Criteria still met or degraded as specified | O2, O6 |
| T-6 Soak | 50% of peak for 8 hours | No memory growth, no queue build-up | O6 |
| T-7 Recovery drill | F-10 including a lag-induced loss test | RTO at most 30 min, RPO at most 1 min | O6 |
| T-8 Idempotency | Duplicate submissions and replayed messages | One order, one charge | O3 |

Data for tests: at least 100,000 products and 1,000,000 synthetic customers, no real personal data. Tools: TS-19.

**Acceptance:** all T-1 to T-8 pass at least once before first production release (REQ-30, REQ-31). Failures send assumptions back to A3 (objectives) or to this document (sizing) through change control.

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
