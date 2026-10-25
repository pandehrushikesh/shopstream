# G1: Implementation Governance Model

| Field | Value |
|---|---|
| Phase | G |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | P1 principles PR-01 to PR-12, E1, E2, F2 (gates), D5 section 4, ADR-001 to ADR-008, REQ-01 to REQ-35 |

## 1. Purpose

Phase G keeps what gets built consistent with the approved architecture. On a one-person project it must be light enough to use and strict enough to mean something. Governance here means three things: **a way to check** (compliance reviews), **a way to record change** (ADRs and a change log) and **a way to say no for now** (gates).

## 2. Governance roles

One person holds all roles, but the roles stay distinct so the checks are real. The rule: **the reviewer role signs off after a pause and against the written checklist, never from memory.**

| Role | Duty | Applied when |
|---|---|---|
| Developer | Builds the package to the definition of done | Every package |
| Architect | Compliance review against G2 checklist | At each transition architecture and each spike close |
| Sponsor | Approves deviations, scope changes and gates | On request from the Architect |

## 3. Architecture contracts

A contract is what a work package promises the architecture. It is recorded as a short section at the top of the package's pull request or folder README.

| Contract item | Content |
|---|---|
| Scope | Work package ID and the requirements it satisfies (E1 section 5) |
| Boundaries | Data it owns, APIs and events it publishes (PR-03, PR-05) |
| Quality | SLOs and the tests that evidence them (D4, D5) |
| Security | Data class declared, scopes used, threat model note (D3) |
| Operations | Health endpoints, dashboards, runbook, rollback (D5 section 4) |

## 4. Definition of done (per service, from D5 section 4)

A service may go live only with all of:
1. Health endpoints (live and ready).
2. Dashboards, SLOs and actionable alerts (REQ-32).
3. A runbook and a named owner.
4. Load-test evidence relevant to its tier (REQ-30).
5. Threat-model review and data-class declaration (D3).
6. Contract tests for every API and message it exposes (PR-05).
7. A proven rollback.
8. The Interview Companion documents are updated: evidence tags moved, assumed numbers replaced by measured ones, new stories and questions added (CH-005). Applies at every gate closure, not per service.

A work package that is not a service (platform work, spikes) uses the relevant subset, stated in its contract.

## 5. Decision records and change control

| Item | Rule |
|---|---|
| ADR | Any decision that affects structure, a technology choice, a boundary or a quality target gets an ADR (PR-12). Files in `docs/adr/` |
| ADR statuses | Proposed, Accepted, Superseded, Rejected. Only the sponsor moves a record to Accepted |
| Superseding | A changed decision gets a new ADR that points to the old one. Old ADRs are not edited except for status |
| Requirement change | Edit the requirement in its source document with a revision-history line, update the register, note the effect on tests |
| Document versioning | Minor version while drafting, v1.0 when approved. A change after approval needs a new version and a log line |
| Spike outcomes | Recorded as ADR notes and, where they change an assumption, in A5 |

## 6. Deviations and dispensations

A deviation is departing from an approved decision during build.

| Step | Detail |
|---|---|
| 1 | Record it: what, why, which requirement or ADR, expected duration |
| 2 | Classify: **temporary** (has an end date and a plan to comply) or **permanent** (needs a new or superseding ADR) |
| 3 | Sponsor approves a dispensation (temporary) or the new ADR (permanent) |
| 4 | Track in the deviation log (G2 section 4). Review at each gate |

A dispensation never covers security requirements REQ-26 to REQ-29, or the correctness rules BR-01 to BR-12 for stock and payment.

## 7. Gates

The gates from the roadmap (F2 section 1) become review points. Each gate has an entry condition (what must exist) and an exit decision (go, go with conditions, or hold).

| Gate | After | Review content |
|---|---|---|
| G-0 | TA-0 | CI green on .NET 10, repository hygiene, no secrets in history |
| G-1 | Spikes SP-3, SP-1 | Spike pass/fail recorded, fallbacks decided |
| G-2 | TA-1 | Security (REQ-26 to 29), observability baseline, gateway limits |
| G-3 | TA-2 | Catalogue contract, shadow/canary evidence, first stopping point review |
| G-4 | TA-3 | Basket contract and switch result |
| G-5 | SP-2 | Hot-item strategy ADR |
| G-6 | Before wave 3 | T-1 to T-8 and T-7 drill evidence (F1 section 6), ACT-1 done |
| G-7 | TA-4, TA-5 | Saga correctness, reconciliation results, retirement readiness |
| G-8 | TA-6 | Final report: objectives O1 to O6 evidence |

## 8. Metrics (kept small)

| Metric | Why |
|---|---|
| Requirements with passing evidence / total (of 35) | Shows real conformance |
| Open deviations and age | Detects drift |
| Deployment lead time (O4 target one day) | Evidence for O4 |
| Open ADRs in Proposed state older than a week | Prevents undecided drift |

## 9. Risks

| ID | Type | Statement | Response |
|---|---|---|---|
| R-26 | Risk | Self-review becomes a rubber stamp | Checklist-based, paused review, evidence links required |
| R-27 | Risk | Governance overhead slows a one-person project | Keep checklist short, review only at gates |

## 10. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve the roles and the "pause and checklist" rule (section 2) | **Approved** |
| 2 | Approve architecture contracts and definition of done (sections 3, 4) | **Approved** |
| 3 | Approve ADR, change and deviation rules (sections 5, 6) | **Approved** |
| 4 | Approve gates G-0 to G-8 (section 7) | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor (all three governance roles) |
