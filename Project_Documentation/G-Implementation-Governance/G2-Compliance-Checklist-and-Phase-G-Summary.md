# G2: Compliance Checklist and Phase G Summary

| Field | Value |
|---|---|
| Phase | G |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | G1, REQ-01 to REQ-35 (sources: A3, B4, C5, D6), P1 principles |

## 1. How to use this checklist

At each gate (G1 section 7) the Architect opens this document, updates the **Status** column from evidence and links the evidence in the pull request or gate note. Status values: Not started, In progress, Evidenced, Deviation (see section 4). A requirement is **Evidenced** only when the named evidence exists and passes.

## 2. Requirement compliance register

| Req | Requirement (short) | Evidence | Checked at | Status |
|---|---|---|---|---|
| REQ-01 | Browse and search under peak load | T-1, T-2 report | G-3 (spot), G-6 | Not started |
| REQ-02 | Checkout idempotent | T-8, integration test | G-7 | Not started |
| REQ-03 | No oversell | T-3, concurrency test | G-5, G-6 | Not started |
| REQ-04 | Order status visible, honest on failure | UI test, saga state test | G-7 | Not started |
| REQ-05 | All external calls authenticated, scopes for state changes | Gateway and service auth tests | G-2 | Not started |
| REQ-06 | Independent deployment | One service released alone in pipeline | G-3 | Not started |
| REQ-07 | Health and business metrics with alerts | Dashboards and alert test | G-2 | Not started |
| REQ-08 | No card data stored or logged | Log and database scan | G-2, G-7 | Not started |
| REQ-09 | Offers with quantity and per-customer limit | Catalogue and Inventory tests | G-5 | Not started |
| REQ-10 | Reservations expire | Time-based test | G-7 | Not started |
| REQ-11 | Customer cancellation before confirmation | Saga test | G-7 | Not started |
| REQ-12 | Daily reconciliation report | Report sample | G-7 | Not started |
| REQ-13 | Order timeline for support | Timeline view test | G-7 | Not started |
| REQ-14 | Notifications on confirm or cancel | Consumer test | G-7 | Not started |
| REQ-15 | Unknown payment outcomes resolved | Failure-injection test | G-6 | Not started |
| REQ-16 | Per-customer checkout key | T-8 | G-7 | Not started |
| REQ-17 | Outbox (atomic state and event) | Crash test from SP-1 | G-1 | Not started |
| REQ-18 | Inbox (safe repeat consumers) | Duplicate delivery test | G-1 | Not started |
| REQ-19 | Degradation as specified | Failure injection F-1 to F-9 | G-6 | Not started |
| REQ-20 | Data classes enforced in logs and messages | Log review, schema review | G-2 | Not started |
| REQ-21 | Multi-region-ready design | Design review (global ids, UTC) | G-3, G-7 | Not started |
| REQ-22 | Versioned APIs and messages, contract tests | Contract test suite | G-3 | Not started |
| REQ-23 | Hot-item reservations without oversell | SP-2 result, T-3 | G-5, G-6 | Not started |
| REQ-24 | Liveness, readiness, correlation ids | Endpoint and trace check | G-2 | Not started |
| REQ-25 | Gateway limits with 429 and Retry-After | SP-3, T-4 | G-1, G-2 | Not started |
| REQ-26 | Services validate tokens and scopes | Test bypassing gateway | G-2 | Not started |
| REQ-27 | No secrets in source, images, logs | Secret scan in CI | G-0 | Not started |
| REQ-28 | Scanned, signed images with SBOM | Pipeline evidence | G-2 | Not started |
| REQ-29 | Non-root, default-deny network policy | Cluster policy check | G-2 | Not started |
| REQ-30 | T-1 to T-8 pass before first production release | Test report | G-6, G-8 | Not started |
| REQ-31 | Recovery drill meets RTO and RPO | T-7 report | G-6, G-8 | Not started |
| REQ-32 | SLOs, dashboards, alerts for Tier 1 | Dashboard inventory | G-2, G-7 | Not started |
| REQ-33 | Expand and contract changes | Change review sample | G-4, G-7 | Not started |
| REQ-34 | Infrastructure as code, reproducible | Rebuild environment from code | G-2 | Not started |
| REQ-35 | Supported LTS versions | Version check against D2 watchlist | G-0, every gate | Not started |

## 3. Principle checks (applied at every gate)

| Principle | Question to ask |
|---|---|
| PR-01 | Does each new service map to a business capability? |
| PR-02 | Is any new component justified by an objective? |
| PR-03 | Does each data set have one owner, and does no service read another's tables? |
| PR-04 | Is personal and payment data minimised and kept out of logs? |
| PR-05 | Is there a contract and a contract test before the code depends on it? |
| PR-06 | What happens when each dependency fails? Is it tested? |
| PR-07 | Can the operation be repeated safely? |
| PR-08 | Do state changes across boundaries travel asynchronously? |
| PR-09 | Is every boundary authenticated and authorised? |
| PR-10 | Is it observable and automated end to end? |
| PR-11 | Did we adopt before building commodity and security-critical parts? |
| PR-12 | Are decisions recorded and are dates for revisiting set? |

## 4. Deviation log

| ID | Date | Deviation | Requirement or ADR | Type | End date or ADR | Approved by |
|---|---|---|---|---|---|---|
| (none yet) | | | | | | |

## 5. Phase G summary

### 5.1 Risks and assumptions

R-26 and R-27 are added to A5 (v1.5). No new assumptions.

### 5.2 Phase G exit criteria

| Criterion | Status |
|---|---|
| Governance roles, contracts and definition of done set (G1) | Approved |
| Decision, change and deviation rules set (G1) | Approved |
| Gates G-0 to G-8 defined (G1) | Approved |
| Compliance register covering all 35 requirements (G2) | Approved |

### 5.3 Hand-off to Phase H and Requirements Management

| To | Content |
|---|---|
| Phase H (Architecture Change Management) | Triggers for revisiting the architecture: runtime or library end of support (D2 watchlist), load-test results contradicting D4, merge triggers from ADR-004, spike failures, licence changes, new regions |
| Requirements Management | The register in section 2 is the live trace from requirement to evidence |

## 6. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve the compliance register and principle checks (sections 2, 3) | **Approved** |
| 2 | Approve G1 and G2, and close Phase G | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor (all three governance roles) |
