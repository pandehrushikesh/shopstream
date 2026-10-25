# H1: Architecture Change Management and Requirements Management

| Field | Value |
|---|---|
| Phase | H and Requirements Management |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | G1, G2, ADR-001 to ADR-008, D2 section 6 (watchlist), C3 section 8 (what would change the decision), A5 |

## 1. Purpose

Phase H keeps the architecture alive after delivery starts. It answers: what events force a second look, how big is the change, who decides, and how the record is kept. Requirements Management is the continuous thread through all phases, so its rules are set here too.

## 2. Change triggers

| # | Trigger | Source | First check |
|---|---|---|---|
| H-T1 | A runtime, library or platform nears end of support (.NET 10 Nov 2028, MongoDB 8.0 Oct 2029, MassTransit v8 patch window 2026) | D2 watchlist | Quarterly |
| H-T2 | A licence change turns a free component into a paid one | D2 section 3 | Quarterly |
| H-T3 | A load test or spike contradicts a planning assumption (A-10, A-11) | D4, E1 | At each gate |
| H-T4 | Two services change together in over half of their releases in a quarter, or need a shared transaction | ADR-004 merge trigger | Quarterly |
| H-T5 | Platform maturity not reached by first production release | ADR-004 fallback | G-6 |
| H-T6 | A security finding or a threat-model change | D3 | Continuous |
| H-T7 | A new business need: second region, new market, new payment method | A3, C1 | On request |
| H-T8 | A deviation becomes permanent | G1 section 6 | At each gate |
| H-T9 | **Team count or skills change from the assumed four teams.** See section 5 | C3 section 8 | At project start and review |

## 3. Classification and handling

| Class | Meaning | Examples | Handling |
|---|---|---|---|
| Simplification | Fits inside the current architecture | Replace a library behind a port, tune a limit | Developer decides, note in the ADR log if it affects an NFR |
| Incremental | Changes a decision but not the target state | Switch messaging library (SP-1 fail), hot-item strategy (SP-2) | New or superseding ADR, sponsor approval, update affected documents |
| Re-architect | Changes the target state or an objective | Merge services (H-T4), second region, change of broker | Return to the ADM at the affected phase (A for objectives, B/C/D for the layer), then repeat E to G for the delta |

## 4. Change process

| Step | Action |
|---|---|
| 1. Log | Record the trigger and the date in the change log (section 6) |
| 2. Classify | Use section 3 |
| 3. Assess | Effect on objectives O1 to O6, requirements, ADRs, work packages, risks |
| 4. Decide | Developer (simplification), sponsor (incremental, re-architect) |
| 5. Update | Edit documents with new versions and revision lines. Never overwrite history. Supersede ADRs |
| 6. Trace | Update the compliance register (G2 section 2) and the index |
| 7. Review | Look again at the next gate to confirm the change worked |

## 5. A standing observation about this project

The architecture is designed for a fictional company with four teams (B3). The person building it is one developer. The ADR-004 conditions say the choice should be revisited "if team count falls to one or two". That condition is in fact met at the builder level, and the project deliberately accepts it: the purpose is to practise the designed architecture and the ADM, not to optimise delivery for one person.

Consequences, stated openly so they are not forgotten:
- The microservice split is more expensive for one person than a modular monolith would be. The extra cost is accepted as the learning investment (ADR-004 already discloses learning value as a tie-breaker).
- The merge fallback (ADR-004, Option B for Ordering, Basket and Notification) stays available if progress stalls. Using it would be an **incremental** change, recorded by ADR, and would be a good interview story in itself.
- Roles in G1 are exercised by one person, with checklists as the control.

## 6. Change log

| ID | Date | Trigger | Class | Decision | Documents changed |
|---|---|---|---|---|---|
| CH-001 | 2026-10-24 | H-T1/H-T2: .NET 8 end of support and MassTransit v9 licence | Incremental | ADR-005 (.NET 10), ADR-007 (v8 with fallback) | D2, D6, A5, ADRs |
| CH-002 | 2026-10-24 | H-T7: sponsor wants cloud-neutral and one region with recovery site | Incremental | D1 cloud-neutral, warm standby | D1, D4 |
| CH-003 | 2026-10-24 | Sponsor decision on data engines | Incremental | ADR-006: PostgreSQL, MongoDB, Redis-compatible | D2, ADR-006 |

| CH-004 | 2026-10-25 | H-T3: SP-3 result contradicts the implicit assumption that client timeouts bound a store call | Simplification | Production limiter gets a call-site deadline and a half-open breaker. D4 4.1 text (fail-safe, local = global / N) is unchanged and confirmed | SP-3 RESULTS.md, A5 (R-28) |
| CH-005 | 2026-10-25 | Sponsor decision: keep an Interview Companion (three Word documents) as part of the evidence trail | Simplification | Updating the companion documents becomes part of closing each gate (G1 definition of done, item 8). Documents live in `Interview-Companion/` | G1, 00-Index |

(Earlier changes C-01 to C-05 are recorded in B6.)

## 7. Requirements management

Rules that already apply, gathered in one place.

| Rule | Detail |
|---|---|
| Single source | Each requirement lives in the document that introduced it. The compliance register (G2) is the trace to evidence |
| IDs never reused | REQ-01 to REQ-35 are fixed. New requirements continue from REQ-36 |
| Change | Edit in the source document with a revision-history line, update G2, note the effect on tests |
| Retirement | A requirement is retired by a note and a pointer, not deleted |
| Review | At every gate (G1 section 7) the Architect checks that no REQ lacks evidence or an owner |

## 8. Final review of the architecture set (before the first code package)

| Check | Result |
|---|---|
| Every objective O1 to O6 is covered by at least one requirement and test | Pass. Each objective has 3 to 9 requirements (D6) and evidence in G2 |
| Every REQ has a work package (E1 section 5) and an evidence line (G2) | Pass. 35 of 35 in G2; E1 section 5 maps all groups |
| Every ADR has a status and a reference to the documents that justify it | Pass with one open item: ADR-001 is still Provisional (see section 8.1) |
| Index lists every document with a status | Pass. Every document is listed |
| RAID log has an owner for every open risk | Pass. R-01 to R-27 all have owners |

These five checks were run on 2026-10-24, before WP-00. The result is recorded in the index.

### 8.1 Open item

ADR-001 (microservices and polyglot persistence) is the only ADR not Accepted. Its own text says the split was validated by ADR-004, which is Accepted and approved. Recommendation: move ADR-001 to Accepted with a note that ADR-004 and ADR-006 refine it. Sponsor to confirm.

## 9. Decisions requested from the sponsor

| # | Decision | Recommendation |
|---|---|---|
| 1 | Approve triggers and classification (sections 2, 3) | **Approved** |
| 2 | Approve the change process and the change log (sections 4, 6) | **Approved** |
| 3 | Accept the standing observation and its consequences (section 5) | **Approved** |
| 4 | Approve the requirements management rules and the final review (sections 7, 8) | **Approved** |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor. Final review run |
