# ShopStream: Architecture Documentation Index

**Method:** TOGAF 10 Architecture Development Method (ADM)
**Lead architect:** Hrushikesh Pande
**Status of this index:** v1.0, 2026-10-24. Architecture baseline complete: Preliminary to H approved

> ShopStream is a **fictional** e-commerce organisation used as a realistic case study. Stakeholders, figures and constraints are role-played assumptions, labelled as such, and are meant to be challenged and replaced as understanding improves.

## 1. How we are working

Each ADM phase produces a small set of deliverables. A phase is closed only when its exit criteria are met. The ADM is iterative: later phases may send corrections back to earlier documents, and every change is logged in that document's revision table.

## 2. ADM progress

| Phase | Name | Status |
|---|---|---|
| Prelim | Preliminary (principles, governance, tailoring) | **Approved v1.0** |
| A | Architecture Vision | **Approved v1.0** |
| B | Business Architecture | **Approved v1.0** |
| C | Information Systems Architectures (Data, Application) | **Approved v1.0** |
| D | Technology Architecture | **Approved v1.0** |
| E | Opportunities and Solutions | **Approved v1.0** |
| F | Migration Planning | **Approved v1.0** |
| G | Implementation Governance | **Approved v1.0** |
| H | Architecture Change Management | **Approved v1.0** |
| Req. Mgmt | Requirements Management (continuous) | Active: REQ-01 to REQ-35 (B6 traces 01 to 15, C5 adds 16 to 24, D6 adds 25 to 35) |

Final review (H1 section 8) passed on 2026-10-24 with one open item: ADR-001 status.

## 3. Document register

| ID | Document | Phase | Status |
|---|---|---|---|
| P1 | [Architecture Principles](Preliminary/P1-Architecture-Principles.md) | Prelim | Approved |
| A1 | [Request for Architecture Work](A-Architecture-Vision/A1-Request-for-Architecture-Work.md) | A | Approved |
| A2 | [Statement of Architecture Work](A-Architecture-Vision/A2-Statement-of-Architecture-Work.md) | A | Approved |
| A3 | [Architecture Vision](A-Architecture-Vision/A3-Architecture-Vision.md) | A | Approved |
| A4 | [Stakeholder Map](A-Architecture-Vision/A4-Stakeholder-Map.md) | A | Approved |
| A5 | [RAID Log](A-Architecture-Vision/A5-RAID-Log.md) | A | Approved (living) |
| B1 | [Business Capability Model](B-Business-Architecture/B1-Business-Capability-Model.md) | B | Approved |
| B2 | [Value Streams](B-Business-Architecture/B2-Value-Streams.md) | B | Approved |
| B3 | [Organisation and Roles](B-Business-Architecture/B3-Organisation-and-Roles.md) | B | Approved |
| B4 | [Business Process Models and Rules](B-Business-Architecture/B4-Business-Process-Models.md) | B | Approved |
| B5 | [Business Information Concepts](B-Business-Architecture/B5-Business-Information-Concepts.md) | B | Approved |
| B6 | [Gap Analysis, Impact and Phase B Summary](B-Business-Architecture/B6-Business-Gap-Analysis-and-Impact.md) | B | Approved |
| C1 | [Data Architecture](C-Information-Systems-Architecture/C1-Data-Architecture.md) | C | Approved |
| C2 | [Application Architecture](C-Information-Systems-Architecture/C2-Application-Architecture.md) | C | Approved |
| C3 | [Architecture Style Evaluation](C-Information-Systems-Architecture/C3-Architecture-Style-Evaluation.md) | C | Approved |
| C4 | [Interfaces, Events and Checkout Saga](C-Information-Systems-Architecture/C4-Interfaces-and-Saga-Design.md) | C | Approved |
| C5 | [Gap Analysis and Phase C Summary](C-Information-Systems-Architecture/C5-Gap-Analysis-and-Phase-C-Summary.md) | C | Approved |
| D1 | [Platform and Cloud-Neutral Approach](D-Technology-Architecture/D1-Platform-and-Cloud-Neutral-Approach.md) | D | **Approved v1.0** |
| D2 | [Technology Selection and Standards](D-Technology-Architecture/D2-Technology-Selection-and-Standards.md) | D | **Approved v1.0** |
| D3 | [Security Architecture](D-Technology-Architecture/D3-Security-Architecture.md) | D | **Approved v1.0** |
| D4 | [Capacity, Resilience and Recovery](D-Technology-Architecture/D4-Capacity-Resilience-and-Recovery.md) | D | **Approved v1.0** |
| D5 | [Observability and Delivery](D-Technology-Architecture/D5-Observability-and-Delivery.md) | D | **Approved v1.0** |
| D6 | [Gap Analysis and Phase D Summary](D-Technology-Architecture/D6-Gap-Analysis-and-Phase-D-Summary.md) | D | **Approved v1.0** |
| E1 | [Work Packages and Spikes](E-Opportunities-and-Solutions/E1-Work-Packages-and-Spikes.md) | E | **Approved v1.0** |
| E2 | [Solution Choices and Phase E Summary](E-Opportunities-and-Solutions/E2-Solution-Choices-and-Phase-E-Summary.md) | E | **Approved v1.0** |
| F1 | [Migration Strategy and Wave Plan](F-Migration-Planning/F1-Migration-Strategy-and-Wave-Plan.md) | F | **Approved v1.0** |
| F2 | [Roadmap, Rehearsal and Phase F Summary](F-Migration-Planning/F2-Roadmap-Rehearsal-and-Phase-F-Summary.md) | F | **Approved v1.0** |
| G1 | [Governance Model](G-Implementation-Governance/G1-Governance-Model.md) | G | **Approved v1.0** |
| G2 | [Compliance Checklist and Phase G Summary](G-Implementation-Governance/G2-Compliance-Checklist-and-Phase-G-Summary.md) | G | **Approved v1.0** |
| H1 | [Change Management and Requirements Management](H-Architecture-Change-Management/H1-Change-Management-and-Requirements-Management.md) | H | **Approved v1.0** |

## 4. Identifier conventions

Stable IDs make traceability possible: *driver -> goal -> objective -> requirement -> component*.

| Prefix | Meaning | Defined in |
|---|---|---|
| D | Business driver | A3 |
| G | Goal | A3 |
| O | Measurable objective (KPI) | A3 |
| PR | Architecture principle | P1 |
| S | Stakeholder | A4 |
| R / A / I / DEP | Risk / Assumption / Issue / Dependency | A5 |
| CAP / VS / PRC | Capability / Value stream / Process | B1 / B2 / B4 |
| TP / TS / OD | Technology principle / standard / open decision | D1 / D2 |
| SO / DS / T / F | Security objective / security decision / threat or test / failure scenario | D3, D4 |
| DD | Design decision carried forward | C4, D4 |
| BR / BP | Business rule / Baseline problem | B4 |
| REQ | Requirement | A3, B4, traced in B6 |
| C | Recorded change to an earlier document | B6 |
| ADR | Architecture decision record | `docs/adr` in the code repository |

## 5. Relationship to the existing code and ADRs

A first code spike (Catalog service on MongoDB, with ADR-001 to ADR-003) was built before this ADM cycle began. Under TOGAF it is treated as a **technology probe**: useful evidence, not an authoritative architecture. Status after Phase C: ADR-001 Provisional (service split now validated by ADR-004, store choices await Phase D), ADR-002 Provisional (Phase D), ADR-003 Accepted. ADR-004 (architecture style) was added in Phase C. Development of further services pauses until Phase D exit.

## 6. Conventions

- Documents are Markdown so they are versioned with the code and render on GitHub; Mermaid is used for diagrams.
- Each document has a revision table. Open questions are listed explicitly rather than hidden in prose.
