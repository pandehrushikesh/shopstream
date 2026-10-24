# SP-3: Gateway rate limiting and admission

**Question (E1):** Can a YARP-based gateway enforce the D4 limits across several instances using atomic counters in a Redis-compatible store, and degrade safely when the store is down?

**Time box:** 2 working days. **Requirement:** REQ-25. **Gate:** G-1.

## What is here

| Path | Content |
|---|---|
| `Gateway/` | Minimal gateway: YARP, middleware that applies D4 4.2 limit classes, Redis Lua limiters, local fallback |
| `Tests/` | xUnit tests against a real Redis in Docker (Testcontainers) |
| `k6/limits.js` | Load scenarios: shared limits, store outage, global checkout admission |

Limiters: token bucket, sliding window (login) and in-flight concurrency (place order). All shared state lives in Redis and is updated by one Lua script per decision, so it is atomic. Time comes from the store (Redis `TIME`) to avoid clock skew between gateways.

Fallback: when the store fails, each gateway switches to local limits for 5 seconds (limit divided by number of instances), then tries the store again.

## Spike simplifications (not production)

- The user id comes from the `X-Spike-User` header. The real gateway uses the validated session or token (ADR-008).
- `X-Forwarded-For` is trusted. Production must trust only the edge proxy.
- Local fallback state has no eviction. Production needs expiry.
- Scripts use EVAL, not EVALSHA. Production should preload scripts.

## Step 1: unit and integration tests (Docker must be running)

```
dotnet test spikes/SP3-RateLimiting/Tests/ShopStream.Spike.Gateway.Tests.csproj
```

Expect 7 tests passing. They prove: burst then reject with Retry-After, refill, the limit holds across two limiter instances, fallback divides the limit by the instance count, sliding window, concurrency, recovery.

## Step 2: run three gateways against one Redis

```
cd deploy
docker compose up -d redis
cd ..
```

Open three terminals in the repo root:

```
dotnet run --project spikes/SP3-RateLimiting/Gateway --urls http://localhost:5201 --RateLimiting:InstanceName=gw1
dotnet run --project spikes/SP3-RateLimiting/Gateway --urls http://localhost:5202 --RateLimiting:InstanceName=gw2
dotnet run --project spikes/SP3-RateLimiting/Gateway --urls http://localhost:5203 --RateLimiting:InstanceName=gw3
```

Smoke test: `curl -i http://localhost:5201/spike/ping`. Look for the headers `X-RateLimit-Source: Redis` and `X-Gateway-Instance: gw1`.

## Step 3: load scenarios (install k6 first: `winget install k6 --source winget`)

| Scenario | Command | Pass criteria |
|---|---|---|
| Shared limit | `k6 run -e SCENARIO=shared spikes/SP3-RateLimiting/k6/limits.js` | `allowed` about 660 (20/s for 30 s plus burst 60), not about 2,000 |
| Store outage | `k6 run -e SCENARIO=outage ...`, and **while it runs** (after about 20 s) `docker stop shopstream-redis-1`, later `docker start shopstream-redis-1` | `decided_by_local` rises after the stop. Total rate stays near the same figure. No 5xx. Returns to `decided_by_redis` after the restart |
| Admission | `k6 run -e SCENARIO=admission ...` | Accepted about 18,000 of about 27,000. 429 `reject_latency_ms` p95 below 20 ms |

The Docker container name can differ: check with `docker ps`.

## Step 4: record the result

Paste the k6 summary (the `allowed`, `limited`, `decided_by_*` lines and the p95 line) and the test output. I will turn them into the decision note for REQ-25 and update the gate.

## Questions this spike should answer

1. Do the limits hold within a small tolerance across three instances? (Shared scenario)
2. Does the gateway stay up and fail safe when Redis dies, without failing open on checkout? (Outage scenario; place-order uses the same fallback)
3. Is the cost of one Lua call per request acceptable? (Admission scenario p95, plus CPU on the Redis container)
4. What did we not cover? (Hot-key behaviour on the global key `checkout-admission:all`, which sees every order: record the Redis CPU during the admission run.)
