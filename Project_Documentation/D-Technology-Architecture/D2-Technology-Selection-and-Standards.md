# D2: Technology Selection and Standards Catalogue

| Field | Value |
|---|---|
| Phase | D |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | D1, C1 to C4, P1 (PR-02, PR-11), RAID log |
| Decision records | `docs/adr/005` to `docs/adr/008` |

## 1. Purpose
Record the product and standard choices, why they were made, and their lifecycle and licence position, so that nothing is chosen by habit. Statuses: **Adopt** (decided), **Proposed** (needs sponsor confirmation), **Open** (decision deferred with a plan).

## 2. Verified lifecycle facts (checked 2026-10-24)

| Fact | Consequence |
|---|---|
| .NET 8 (LTS) and .NET 9 both reach **end of support on 10 November 2026**. .NET 10 is LTS, supported through **November 2028** ([Microsoft .NET blog](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)) | New work must target .NET 10. The earlier code spike targets .NET 8 and must be retargeted (issue I-03) |
| MassTransit v9 is a **commercial product** ([MassTransit support page](https://masstransit.massient.com/reference/support)). A third-party article reported that v8 stays open source with security patches planned "through 2026" and v9 pricing proposed but not final ([Milan Jovanovic](https://www.milanjovanovic.tech/blog/mediatr-and-masstransit-going-commerical-what-this-means-for-you)) | Using v8 for a new build means starting on a line whose planned patch window ends this year. v9 needs a paid licence. See OD-1 |

Pricing and v8 timelines are as reported by the cited sources and must be re-checked at decision time.

## 3. Standards catalogue

| ID | Category | Choice | Status | Rationale and alternatives | Licence / lifecycle note | Lock-in |
|---|---|---|---|---|---|---|
| TS-01 | Runtime | .NET 10 (LTS), C# | **Adopt** | LTS through Nov 2028. Team expertise. Alternatives rejected: staying on .NET 8 or 9 (support ends 10 Nov 2026) | Open source | Low |
| TS-02 | Service style | ASP.NET Core minimal APIs, OpenAPI | **Adopt** | Light, fast, fits contract-first (PR-05) | Open source | Low |
| TS-03 | Storefront | React with TypeScript | **Adopt** | Mainstream, strong ecosystem | Open source | Low |
| TS-04 | Containers and orchestration | OCI images, Kubernetes | **Adopt** | Portability layer (D1). Alternative: provider-specific container services (rejected: lock-in) | Open source | Low |
| TS-05 | Gateway | YARP | **Adopt** | Fits the .NET team, programmable, supports rate limiting and authentication middleware. Alternatives: commodity gateways (more features, more to operate) | Open source, Microsoft-maintained. Confirm .NET 10 support at implementation | Low |
| TS-06 | Identity | Keycloak | **Adopt** (confirms ADR-002) | Standards-based, self-hostable, avoids building security. Alternatives: Duende, OpenIddict (build and licence trade-offs, ADR-002) | Open source | Low |
| TS-07 | Relational data | PostgreSQL | **Accepted** (ADR-006) | Covers Ordering, Inventory, Payment, Keycloak with one engine | Open source | Low |
| TS-08 | Document data | MongoDB | **Accepted** (ADR-006) | Catalogue (see section 4) | Source-available licence. Managed alternatives differ in compatibility | Medium |
| TS-09 | Cache and state | Redis-compatible | **Adopt** | Basket, cache, limiter and gateway session state | Check the licence of the chosen Redis-compatible product at selection | Low |
| TS-10 | Broker | RabbitMQ, quorum queues | **Adopt** (ADR-007) | Command and event routing, dead-lettering, adequate throughput (D4). Alternative: Kafka (kept as scale-out option) | Open source | Low |
| TS-11 | Messaging library | MassTransit v8, free fallback (OD-1) | **Accepted with risk** | Library for outbox, inbox, saga | See section 5 | Medium |
| TS-12 | Telemetry | OpenTelemetry, OTLP | **Adopt** | Vendor-neutral | Open source | Low |
| TS-13 | Telemetry backends | Prometheus, Grafana, Loki, Tempo (self-run or managed equivalents) | **Adopt** | Open stack, portable. Local development may use any OTLP-compatible viewer | Open source | Low |
| TS-14 | Resilience | Microsoft.Extensions.Resilience (Polly) | **Adopt** | Standard .NET resilience pipelines | Open source | Low |
| TS-15 | CI | GitHub Actions | **Adopt** | Repository is on GitHub | Hosted service. Pipelines kept simple to ease migration | Medium |
| TS-16 | Delivery | GitOps controller (Argo CD or Flux), Helm or Kustomize | **Adopt** | Declarative, auditable deploys. Either controller acceptable | Open source | Low |
| TS-17 | Infrastructure as code | OpenTofu/Terraform | **Adopt** | Provider-agnostic workflow | Open source (OpenTofu) | Low |
| TS-18 | Secrets | Secret manager plus external-secrets mechanism | **Adopt** | Keeps secrets out of Git (REQ-27) | Provider choice later | Low |
| TS-19 | Testing | xUnit, Testcontainers, consumer-driven contract tests, k6 for load, chaos tooling for failure tests | **Adopt** | Matches test strategy (D4, D5) | Open source | Low |
| TS-20 | Security scanning | Dependency, container, secret and infrastructure scanning in CI | **Adopt** | REQ-28 | Tools chosen at implementation | Low |

Libraries that moved to commercial licences (for example MassTransit v9, MediatR) count as **paid dependencies**: each needs explicit approval. C2 already avoids MediatR.

## 4. Data engines (ADR-006, Accepted)

Phase C asked Phase D to justify every engine (R-13).

| Option | Engines | Assessment on business criteria | Learning / portfolio value |
|---|---|---|---|
| **1. Three engines** | PostgreSQL (Ordering, Inventory, Payment, Keycloak), MongoDB (Catalogue), Redis (Basket, cache, limiter, sessions) | Good fit for every access pattern. Operational cost of three skills | High: polyglot persistence, keeps the existing Catalogue spike |
| 2. Two engines | PostgreSQL with JSON columns for Catalogue, Redis | Also meets the Catalogue needs given caching and 10k to 100k products. Simpler to run (PR-02) | Lower |
| 3. Four engines | As option 1 plus SQL Server for Ordering | Adds a fourth skill and licence cost for no objective gain, and is less portable | Familiar to the team |

On business criteria alone, option 2 narrowly wins on simplicity, and option 1 is close behind. **Proposed: option 1**, with the learning value stated openly as the tie-breaker, consistent with ADR-004. Option 3 is rejected: it adds cost without serving any objective. This changes the earlier informal plan (Ordering on SQL Server) and needs the sponsor's confirmation.

Exit path for MongoDB: Catalogue's data access sits behind a repository port (C2), so option 2 remains available if the third engine proves too costly.

## 5. Decision OD-1: messaging library for outbox, inbox and saga (closed with accepted risk)

The broker (RabbitMQ) is decided. The **library** that implements outbox, inbox, retries and the Ordering saga is not.

| Option | Pros | Cons |
|---|---|---|
| MassTransit v9 | Mature saga and outbox support | Commercial licence |
| MassTransit v8 | Open source | Patch window reported to end in 2026. Poor choice for a new build |
| Other open-source .NET messaging libraries | Free | Each needs a licence and maturity check, and a saga fit check |
| Thin in-house layer on the RabbitMQ client | No licence, full control, educational | We build and test outbox, inbox, retries and saga ourselves (risk R-12) |

**Sponsor decision (2026-10-24):** no paid licence. Start with **MassTransit v8**. If it proves painful, search for a similar free alternative. **Accepted risk:** the reported patch window ends in 2026 and .NET 10 compatibility is unverified.

Direction: keep **our own messaging port** (so application code never references a vendor library), and run a **time-boxed spike in Phase E** comparing one open-source library and the thin in-house layer against REQ-17, REQ-18 and the saga in C4. The spike now first confirms v8 on .NET 10 and defines the switch criteria; the free alternatives remain the fallback.

## 6. Technology lifecycle watchlist

| Item | Date | Action |
|---|---|---|
| .NET 8 and .NET 9 end of support | 10 Nov 2026 | Retarget spike to .NET 10 at the next code phase |
| .NET 10 end of support | Nov 2028 | Plan next LTS move in 2028 |
| MassTransit v8 patch window (as reported) | End of 2026 | Accepted risk. Review at spike and before any production use |

## Sources
- [.NET 8 and 9 end of support, Microsoft .NET blog](https://devblogs.microsoft.com/dotnet/dotnet-8-9-end-of-support/)
- [MassTransit support page](https://masstransit.massient.com/reference/support)
- [MediatR and MassTransit going commercial, Milan Jovanovic](https://www.milanjovanovic.tech/blog/mediatr-and-masstransit-going-commerical-what-this-means-for-you)

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
