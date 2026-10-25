# SP-3 results: gateway rate limiting and admission

| Field | Value |
|---|---|
| Spike | SP-3 (E1 section 3) |
| Date | 2026-10-25 |
| Requirement | REQ-25 |
| Outcome | **Pass.** Proceed to WP-05 with the design below |
| Environment | One developer machine (16 CPUs): three gateway processes, one Redis container, k6 and the tests, all on the same host. Figures show the logic, not production latency |

## Pass criteria and results

| Criterion (E1) | Result |
|---|---|
| Limits hold across several instances | **Pass.** One IP at 150 req/s for 30 s through three gateways: 659 allowed, expected about 660 (20/s plus burst 60). Not 3 times that |
| Global admission holds | **Pass.** 900 orders/s against the 600/s limit for 30 s: 18,076 accepted (601/s), expected about 18,100 |
| Excess answered quickly | **Pass.** 429 reject latency p95 4.56 ms, max 19.3 ms (target under 20 ms, a local-machine figure) |
| Safe fallback when the store is down, no fail-open | **Pass after two fixes.** Store stopped for about 20 s during 150 req/s: 3,578 decisions made locally, no request failed with a server error, no dropped iterations, max latency 196 ms, returned to the store afterwards |
| Unit and integration tests | 7 of 7 pass against a real Redis (Testcontainers) |

## Findings that change the production design (WP-05)

| # | Finding | Evidence | Design consequence |
|---|---|---|---|
| F-1 | The Redis client enforces its own timeouts on a heartbeat of about one second, so a call on a connection that just died can hang for seconds | First outage run: p95 456 ms, max 6 s, 746 dropped iterations | Put a **hard deadline at the call site** (100 ms `StoreTimeout`, `Task.WaitAsync`). Do not rely on the client's timeout. Run after fix: max 196 ms |
| F-2 | A plain breaker lets every request test the store at once when it re-opens | Second run: p95 3.3 ms but max still 6 s, 189 dropped | Use a **half-open breaker**: one probe request at a time, all others stay local |
| F-3 | Switching modes grants extra burst: local buckets start full and the shared bucket refills to its cap while the store is down | Outage run allowed 1,436 vs about 1,260 to 1,350 expected | Accept (bounded to about 2 bursts per outage) for browse limits. For checkout admission consider starting local buckets empty or half full |
| F-4 | The local fallback divides by a configured instance count. Autoscaling changes the real count | By design (D4 4.1) | Take the divisor from the deployment (for example the pod count) and refresh it. Recorded as risk R-28 |
| F-5 | Local fallback state has no eviction and scripts use EVAL | Spike code | Add expiry. Preload scripts (EVALSHA) |
| F-6 | One global key (`checkout-admission:all`) received 900 requests/s on a single Redis without trouble | Admission run | Hot-key limit not found. If higher rates are needed, split the global bucket into N sub-buckets. Redis CPU was not measured |

## What this spike did not cover

- Real session and token validation (the spike used a header for the user id)
- Real network latency, TLS, and a managed Redis with failover (Sentinel or Cluster)
- Canary routing through the gateway (assumption A-13 is still open)
- Redis CPU and memory under load
- Behaviour with more than three gateways, or with autoscaling during an outage

## Decisions for WP-05

1. Implement per-class limits as in D4 4.2, backed by Redis Lua scripts, with the production changes F-1, F-2, F-4, F-5.
2. Keep fail-safe (local limit = global / N) and the 429 with Retry-After and problem-details body.
3. Load-test T-4 (admission) at staging scale before the first production release (REQ-30).
