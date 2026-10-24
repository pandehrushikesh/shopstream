# P1: Architecture Principles

| Field | Value |
|---|---|
| Phase | Preliminary |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |

Principles are enduring rules that guide every later decision. Each follows the TOGAF template: statement, rationale, implications. If a design breaks a principle, the exception must be recorded as an ADR with a reason.

## Business principles

### PR-01 Services follow business capabilities
- **Statement:** Service boundaries follow business capabilities (Catalog, Basket, Ordering, Inventory, Payment), not technical layers or teams' convenience.
- **Rationale:** Capability boundaries change at the same rate as the business and minimise cross-service coupling.
- **Implications:** Capability map (A3) drives decomposition in Phase C. A new service needs a named capability and an owner.

### PR-02 Complexity must earn its place
- **Statement:** Prefer the simplest architecture that meets the objectives. Every added moving part needs a measurable objective it serves.
- **Rationale:** Microservices carry real operational cost. A modular monolith is the default alternative and must be considered and rejected on evidence.
- **Implications:** Phase C/D must include a monolith-versus-services comparison against objectives O1 to O6.

## Data principles

### PR-03 One owner per data set
- **Statement:** Every data entity has exactly one owning service. Others use it only through that service's API or published events.
- **Rationale:** Shared databases create hidden coupling and block independent change.
- **Implications:** No cross-service joins or shared schemas. Reporting needs a separate read model. Consistency across services is eventual and is managed explicitly (sagas, outbox).

### PR-04 Personal and payment data is minimised and protected
- **Statement:** Collect only what is needed. Never store card data (use PSP tokenisation). Personal data is classified, encrypted in transit and at rest, and has a retention rule.
- **Rationale:** Reduces PCI DSS scope and privacy exposure (DPDP Act 2023, GDPR for EU customers).
- **Implications:** Data classification is a Phase C deliverable. Logs must not contain personal or payment data.

## Application principles

### PR-05 Contract first
- **Statement:** APIs and events are designed and versioned as contracts before implementation. Breaking changes require a new version.
- **Rationale:** Independent deployment is only safe when contracts are explicit and tested.
- **Implications:** OpenAPI for synchronous APIs, a schema registry or schema-in-repo for events, contract tests in CI.

### PR-06 Design for failure
- **Statement:** Assume every remote call can be slow, fail, or be repeated. Timeouts, retries with backoff, circuit breakers, and graceful degradation are mandatory on remote calls.
- **Rationale:** At distributed scale partial failure is normal.
- **Implications:** Resilience policy is defined per dependency. Failure injection is part of acceptance testing.

### PR-07 Operations are idempotent
- **Statement:** Any operation that changes state and may be retried (checkout, payment, message handling) must be safe to repeat.
- **Rationale:** Retries and at-least-once messaging are unavoidable. Duplicate charges or orders are unacceptable (O3).
- **Implications:** Idempotency keys on commands, inbox/outbox patterns, deduplication in consumers.

### PR-08 Asynchronous across boundaries for state change
- **Statement:** Cross-service state changes use asynchronous messaging. Synchronous calls are for queries and for latency-critical, failure-tolerant interactions.
- **Rationale:** Limits temporal coupling and lets the order path absorb bursts.
- **Implications:** Message broker is a core platform component. Eventual consistency must be visible in UX (for example "order received").

## Technology principles

### PR-09 Secure by default, verified at every boundary
- **Statement:** Authenticate at the edge and validate again inside each service (defence in depth). Least privilege everywhere, including service-to-service.
- **Rationale:** A compromised edge or internal network must not mean full compromise.
- **Implications:** OIDC/OAuth2 with short-lived tokens, scopes per service, secrets never in source control, threat model in Phase D.

### PR-10 Observable and automated
- **Statement:** Every service ships with logs, metrics, traces, and health endpoints. Build, test, deploy, and infrastructure are automated and reproducible.
- **Rationale:** Objectives O4 and O6 cannot be met without fast feedback and repeatable delivery.
- **Implications:** Correlation IDs across services, standard dashboards, infrastructure as code, a pipeline per service.

### PR-11 Buy or adopt before building commodity and security-critical capabilities
- **Statement:** Use proven products for identity, messaging, payments, and observability. Build only what differentiates the business.
- **Rationale:** Lower risk and cost for solved problems.
- **Implications:** Each "build" choice needs justification in an ADR. Vendor lock-in is assessed, not assumed away.

### PR-12 Decisions are recorded and revisable
- **Statement:** Significant decisions are captured as ADRs with context, options, and consequences, and revisited when assumptions change.
- **Rationale:** Prevents re-litigation and preserves reasoning for reviewers and future teams.
- **Implications:** ADR status values: Proposed, Provisional, Accepted, Superseded. Linked from phase documents.

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
