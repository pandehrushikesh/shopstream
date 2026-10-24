# F2: Roadmap, Paper Rehearsal and Phase F Summary

| Field | Value |
|---|---|
| Phase | F |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | F1 (approved), E1 (work packages, transition architectures), D4 (tests), A5 |

## 1. Roadmap

Order of delivery, combining build work (E1) with migration waves (F1). Durations are intentionally absent: sizes in E1 are relative and unmeasured (A-11). Dates are set after TA-0 and TA-1 give real figures.

| Step | Transition architecture | Work packages | Migration wave | Gate to pass before the next step |
|---|---|---|---|---|
| 1 | TA-0 | WP-00, WP-01 | none | CI green on .NET 10, stack runs locally |
| 2 | spikes | SP-3, SP-1 | none | Spike decisions recorded (ADR-007 note) |
| 3 | TA-1 | WP-02 to 05 | Wave 0 | Sign-in through gateway, limits enforced, dashboards live |
| 4 | TA-2 | WP-06, 07 | Wave 1 | Shadow then canary criteria met (F1 section 3). **First stopping point** |
| 5 | TA-3 | WP-08 | Wave 2 | Basket switch complete |
| 6 | spike | SP-2 | none | Stock strategy ADR-009 |
| 7 | TA-4 | WP-09 to 12 | Wave 3 | T-1 to T-8 and T-7 recovery drill pass first (F1 section 6) |
| 8 | TA-5 | WP-13, 16 | Wave 4 | One reconciliation cycle with zero differences |
| 9 | TA-6 | WP-14, 15 | proof | Final test and drill report |

WP-14 (load tests) starts as soon as there is something to test, not only at step 9. Steps 7 and 9 overlap in practice, as the F1 hold point requires tests before wave 3.

## 2. Paper rehearsal of Classic retirement (WP-16)

### 2.1 Function-by-function ownership check

| Classic function | New owner | Capability | Wave |
|---|---|---|---|
| Sign-in, accounts | Keycloak with gateway | CAP-09 | 0 |
| Routing, request protection | Gateway | CAP-10 | 0 |
| Product pages, search | Catalogue | CAP-01, 02 | 1 |
| Basket in session | Basket | CAP-03 | 2 |
| Place order | Ordering saga | CAP-04 | 3 |
| Take payment | Payment | CAP-05 | 3 |
| Stock and limits | Inventory | CAP-06 | 3 |
| Order status, history | Ordering | CAP-07 | 3 |
| Emails | Notification | CAP-08 | 4 |
| Logs, monitoring | Observability stack | CAP-11 | 0 |
| Deployment scripts | Pipeline | CAP-12 | 0 |

Result: every Classic function has an owner. No orphan found.

### 2.2 Simulated incidents (decision log)

| # | Wave and stage | Incident | Decision | Rollback or action | Lesson |
|---|---|---|---|---|---|
| I-1 | Wave 1, shadow | Shadow responses differ from Classic on 6% of product pages | Do not enter canary. Investigate | Classify differences: price rounding, missing images, stale data. Fix mapping. Restart the 3-day shadow clock | Agree tolerances and known non-determinism first (R-25) |
| I-2 | Wave 3, canary 10% | Payment provider timeouts rise, many orders in "ResolvingPayment" | Pause the canary at the current share. Do not raise it | Stop new-saga routing for new orders, let in-flight ones finish with unknown-outcome resolution (BR-12), return routing to Classic for new orders. Reconcile every unknown before resuming | Reservation step protects the provider, but visibility of unknown outcomes must be a dashboard and an alert before any canary |
| I-3 | Wave 3, full, day 4 | Daily reconciliation shows 3 units of stock difference on one product | Freeze further product groups. Treat as a defect, not noise | Trace through the outbox and inbox records, compute the correct count, correct by an auditable adjustment. Add a test that reproduces it. Resume only after two clean days | Rollback is no longer practical after days (R-23). Detection speed is the real protection |

Three further incidents are held in reserve for the rehearsal session: password hash import failure (R-24), gateway counter store outage during a canary, and a flash-sale announcement inside the freeze window.

### 2.3 Retention and archive

| Classic data | Where it goes | Period |
|---|---|---|
| Orders and payments | Read-only archive and copy in Ordering for support | C1 retention periods |
| Customer accounts | Keycloak (active) and archive of the hash export, encrypted | Until all accounts have signed in or are closed, then delete |
| Catalogue history | Not retained beyond the last version | n/a |
| Logs | Archive to cheap storage | As per D5 |

### 2.4 Outcome

The rehearsal found no missing function and exposed two actions (below), both added to the register.

## 3. Phase F summary

### 3.1 Gaps closed or carried

| Gap | Status |
|---|---|
| GA-1 One deployable to several services (C5) | Planned: waves 0 to 4 |
| GT-6 Accounts migration (D6) | Planned: hash verify and re-hash, fallback reset |
| Coexistence of Classic and new | Planned (F1 section 7) |
| Real Classic export formats | Open: A-14, paper checklist only |

### 3.2 Actions arising

| ID | Action | Owner |
|---|---|---|
| ACT-1 | Define the unknown-payment dashboard and alert as part of WP-03/WP-11, before any wave 3 canary | Team T4/T3 |
| ACT-2 | Write down shadow-comparison tolerances at the start of WP-06 | Team T1 |

### 3.3 Risks

R-23 to R-25 and A-13, A-14 are already in the RAID log (A5 v1.4). No new risks from the rehearsal.

### 3.4 Phase F exit criteria

| Criterion | Status |
|---|---|
| Migration strategy and principles agreed (F1) | Approved |
| Wave plan with rollback per wave (F1) | Approved |
| Roadmap with gates (F2) | Approved |
| Rehearsal of retirement on paper (F2) | Approved |
| Actions captured | Done |

### 3.5 Hand-off to Phase G (Implementation Governance)

| Item | Content |
|---|---|
| Compliance review | Check each package against REQ-01 to REQ-35 and the ADRs at each transition architecture |
| Architecture contracts | Definition of done (D5 section 4), ADR rule: any deviation is recorded |
| Gates | Gates from section 1 become review points |
| Change process | Requirement and decision changes go through the register and ADR notes |

## 4. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve the roadmap and its gates (section 1) | **Approved** |
| 2 | Accept the rehearsal result and the two actions (section 2, 3.2) | **Approved** |
| 3 | Close Phase F and move to Phase G | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
