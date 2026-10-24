# A2: Statement of Architecture Work

| Field | Value |
|---|---|
| Phase | A |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |

## 1. Purpose
Defines what the architecture engagement will deliver, how it is run, and how it will be judged complete.

## 2. Scope of the architecture work

| Dimension | Decision |
|---|---|
| Breadth | Customer-facing commerce platform: browse, basket, checkout, order, stock, payment, notification, identity, edge, platform operations |
| Depth | Business, Data, Application, Technology architecture to a level that supports implementation. Detailed design only for the critical path (checkout) |
| Time horizon | Baseline (today), one Target Architecture, up to three Transition Architectures |
| Domains | All four (Business, Data, Application, Technology), depth weighted towards Application and Technology |
| Out of scope | Real PSP integration, shipping/logistics, returns, tax engine, multi-vendor marketplace onboarding, recommendations/ML, back-office ERP integration |

## 3. Approach
- ADM iterated in three cycles (below). Each cycle ends with a review against the objectives in A3.
- Work is document-driven first. A small amount of **spike code** may be written to test an architectural risk (for example the saga, or rate limiting at 3,000 RPS). Spikes inform documents; they do not replace them.
- Decisions go into ADRs. Requirements get IDs and are traced to objectives.

| Cycle | ADM phases | Outcome |
|---|---|---|
| 1 | Prelim, A | Agreed vision, principles, scope, stakeholders, risks |
| 2 | B, C, D | Baseline and Target architectures, gap analysis, architecture views |
| 3 | E, F, G, H | Solutions, roadmap in transition states, governance and change process |

## 4. Deliverables by phase

| Phase | Deliverables |
|---|---|
| Prelim | P1 Principles, tailored framework (this document set), repository structure |
| A | A1 Request, A2 this Statement, A3 Architecture Vision, A4 Stakeholder Map, A5 RAID Log, initial requirements list |
| B | Business capability map (detailed), value streams, organisation and roles, business process models (checkout), business gap analysis |
| C | Data entities and ownership, data classification, logical data model per service; application portfolio, interface catalogue, integration (events and APIs), application gap analysis |
| D | Technology reference model, platform choices, security and network architecture, capacity model, resilience and DR design, technology gap analysis |
| E/F | Work packages, transition architectures, roadmap, cost and risk view, migration plan from the monolith |
| G | Architecture contracts, compliance checklist, review records |
| H | Change management process, criteria for minor versus major change |

## 5. Roles and responsibilities

| Role | Held by | Responsibility |
|---|---|---|
| Lead Architect | Hrushikesh Pande | Owns all deliverables, runs reviews |
| Executive sponsor | CTO (role-played) | Approves vision and funding |
| Domain stakeholders | See A4 | Provide requirements, review views |

## 6. Governance and quality
- **Review gates:** each phase closes only on its exit criteria (section 8) and a recorded review.
- **Traceability rule:** every requirement links to an objective. Every component links to a requirement or principle.
- **Change control:** revisions are logged in each document. Principle changes need an ADR.

## 7. Communication plan

| Audience | Content | Form | Frequency |
|---|---|---|---|
| Sponsor | Progress, decisions needed, risks | One-page status | Each phase gate |
| Domain stakeholders | Relevant views and open questions | Review of the documents | Each cycle |
| Delivery teams (future) | Principles, contracts, ADRs | Repository and walkthroughs | Continuous |

## 8. Acceptance criteria (Phase A exit)
1. A3 states the problem, drivers, goals, and measurable objectives, each traceable.
2. Scope and out-of-scope are explicit and agreed.
3. Principles P1 are ratified.
4. Stakeholders and their concerns are listed with engagement approach (A4).
5. Top risks have owners and mitigations (A5).
6. Open questions are listed and none block Phase B.

## 9. Schedule
Effort-based rather than date-based, since this is a learning engagement: Cycle 1 first, then Cycle 2, then Cycle 3. Each cycle is reviewed before the next begins.

## 10. Risks and dependencies
See A5. The main risks are over-engineering relative to need (R-01) and untested capacity assumptions (R-03).

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
