# A1: Request for Architecture Work

| Field | Value |
|---|---|
| Phase | A (input document) |
| Version | 1.0 (Approved 2026-10-24) |
| Date | 2026-10-24 |
| Requesting party | Executive sponsor (role-played): Chief Technology Officer, ShopStream |
| Receiving party | Lead Architect |

> Fictional case study. The sponsor and business context are role-played assumptions.

## 1. Organisation sponsors
- Executive sponsor: Chief Technology Officer.
- Business sponsor: Head of Digital Commerce.

## 2. Organisation's mission and strategic context
ShopStream is an online retailer growing from a regional business into a national marketplace. Revenue depends heavily on campaign and flash-sale events, during which traffic is many times higher than normal. The current platform is a single monolithic application ("ShopStream Classic") that cannot absorb these peaks reliably and is slow to change.

## 3. Request
The sponsor requests an enterprise architecture, developed with the TOGAF ADM, for a new commerce platform that:
1. Sustains peak and flash-sale traffic without degrading the customer experience.
2. Guarantees the integrity of orders, stock, and payments.
3. Lets independent teams change and release parts of the platform safely and frequently.
4. Protects customer and payment data and meets regulatory expectations.
5. Is observable and recoverable in production.

## 4. Business goals and drivers (summary)
Full traceability is in A3. In short: growth through campaigns, customer trust, speed of change, compliance, and cost efficiency.

## 5. Constraints
- Cloud-neutral, container-based deployment.
- Payment card data must not be stored by ShopStream.
- Team of a size comparable to a single product group: operational complexity must stay manageable.
- Existing customer accounts and order history must be migrated (see A3 baseline).

## 6. Expected outputs
Architecture Vision, Statement of Architecture Work, stakeholder map, and risk log for approval, followed by Business, Data, Application, and Technology architectures, an opportunities and migration plan, and implementation governance.

## 7. Approval to proceed
Approval of this request authorises Phase A. Phase B starts only on approval of A2 and A3.

| Role | Name | Decision | Date |
|---|---|---|---|
| Executive sponsor | (role-played) | Pending | |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor and baselined |
