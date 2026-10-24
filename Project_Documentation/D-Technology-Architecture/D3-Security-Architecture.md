# D3: Security Architecture

| Field | Value |
|---|---|
| Phase | D |
| Version | 1.0 (Approved by sponsor, 2026-10-24) |
| Date | 2026-10-24 |
| Owner | Lead Architect |
| Inputs | B3 (roles and access), B5/C1 (data classes), C4, D1, P1 (PR-04, PR-09), objective O5 |
| Decision record | `docs/adr/008-browser-authentication-bff.md` |

> Compliance statements are architectural intentions, not legal advice. Legal and compliance review is required before real use.

## 1. Purpose
Specify how identity, access, data, network and the supply chain are protected, and record a threat model for the checkout path.

## 2. Security objectives (from O5 and D-level needs)

| ID | Objective |
|---|---|
| SO-1 | Only authenticated, authorised users and systems reach protected operations |
| SO-2 | A compromise of one component does not give access to everything (blast radius) |
| SO-3 | No card data is ever stored, logged or transmitted through our systems |
| SO-4 | Personal data is minimised, encrypted and traceable |
| SO-5 | Abuse (bots, credential stuffing, scraping, flooding) is contained at the edge |
| SO-6 | Software and infrastructure are built from trusted, scanned, reproducible inputs |

## 3. Identity and access architecture

### 3.1 Actors and authentication

| Actor | Authentication | Token / session |
|---|---|---|
| Customer in a browser | OpenID Connect authorisation code flow with PKCE, handled by the **gateway acting as backend-for-frontend (BFF)** (ADR-008) | Browser holds only a secure, HTTP-only, same-site session cookie. Access and refresh tokens never reach browser JavaScript |
| Staff (support, finance, merchandiser, campaign manager) | Same flow, plus mandatory multi-factor authentication | Same, shorter session |
| Non-browser API clients (future) | Authorization code or client credentials as appropriate | Bearer access token |
| Service to service (synchronous) | Short-lived tokens obtained with client credentials, one client per service, narrow scopes | Bearer access token |
| Service to broker | Per-service broker account with least-privilege permissions | Credentials from secret manager |
| Service to database | Per-service database user with access to its own schema only | Credentials from secret manager |

Why BFF: tokens stored in browser JavaScript can be stolen by any script injection. Keeping them server-side removes that class of attack. Cost: the gateway needs a shared session store (the Redis-compatible store) and CSRF protection (same-site cookies plus a custom header check).

### 3.2 Token and session parameters `[assumed]`

| Item | Value |
|---|---|
| Access token lifetime | 5 minutes |
| Refresh | Rotated on each use, reuse detected and revoked |
| Browser session idle timeout | 30 minutes (customers), 15 minutes (staff) |
| Signing keys | Rotated regularly, published through the identity provider's key endpoint, cached by services |

### 3.3 Authorisation model

Enforced **twice**: coarse at the gateway (is the caller allowed to reach this route?), fine inside each service (does the caller own this order? has the scope?) (PR-09, REQ-26).

| Scope | Granted to | Allows |
|---|---|---|
| `catalogue:read` | Everyone including anonymous (public routes) | Browse |
| `catalogue:write` | Merchandiser | Edit products and categories |
| `offers:write` | Campaign manager | Create and schedule offers |
| `orders:place` | Customer | Place order, own basket |
| `orders:read` | Customer | Read own orders |
| `orders:read-any` | Support agent, finance analyst, SRE (diagnostic) | Read any order, timeline |
| `orders:cancel-request` | Customer (own, before confirmation), support agent (any) | Request cancellation |
| `reconciliation:read` | Finance analyst | Reconciliation report |
| `platform:admin` | Platform engineer | Platform administration (via platform tooling, not the customer APIs) |

This realises the role-to-access matrix in B3 section 5. Ownership checks (BR-11) are done in Ordering, not by scope alone.

## 4. Data protection

| Topic | Design |
|---|---|
| In transit | TLS at the edge. Between services, TLS using certificates issued automatically inside the cluster is a Phase E item (decision DS-2 in section 9). Payment provider traffic always TLS with provider authentication |
| At rest | Encryption at rest for all data stores and backups. Personal and payment classes use separate storage and keys |
| Card data | Never handled. The payment provider's hosted payment page or hosted fields collect the card. Our systems receive only a token and outcome (REQ-08). This is intended to keep our PCI DSS scope to the lightest category, subject to confirmation with the provider and an assessor |
| Personal data | Collected only as needed. Identifiers in events and logs, never names, emails or addresses (C1 section 8). Support tooling shows personal data only to authorised roles with audit |
| Privacy law | Design supports the India DPDP Act 2023 and GDPR needs: purpose limitation, access and erasure requests (anonymise order references per C1 section 9), breach detection and notification, data residency in India |
| Backups | Encrypted, access restricted, restore tested (D4) |
| Secrets | Held in a secret manager, delivered to workloads at runtime, never in source control, images or logs (REQ-27). Rotation supported without redeploy where possible |

## 5. Network and edge protection

| Control | Design |
|---|---|
| Edge | CDN and web application firewall in front of the load balancer. Basic bot management. DDoS protection from the provider layer |
| Exposure | Only the gateway is reachable from the internet |
| Segmentation | Default-deny network policies. Data stores accept connections only from their owning service |
| Egress | Only allowed destinations (payment provider, identity, telemetry) |
| Rate limiting | Per-IP, per-user, login-specific and checkout admission control (D4 section 4) |
| Admin access | Platform and database administration through authenticated, audited, time-limited access. No standing production credentials on laptops |
| Service mesh | Not adopted initially (PR-02). Revisit if in-cluster mutual TLS and per-call policy become required |

## 6. Supply chain and runtime hardening

| Control | Requirement |
|---|---|
| Source | Branch protection, code review, signed commits where practical |
| Dependencies | Automated vulnerability and licence scanning in CI (TS-20). Commercial-licence dependencies need approval (D2) |
| Images | Minimal base images, built in CI, scanned, accompanied by a software bill of materials, **signed**. Cluster admits only signed images (REQ-28) |
| Runtime | Non-root, read-only file system, dropped capabilities, resource limits (REQ-29) |
| Infrastructure | Defined as code, scanned for misconfiguration, changes through pull requests |
| Patching | Runtime and base images follow supported LTS versions (REQ-35). Critical vulnerabilities patched within an agreed window `[assumed: 7 days]` |

## 7. Audit and detection

| Event | Recorded |
|---|---|
| Sign-in, failed sign-in, token refresh anomalies | Yes, by the identity provider |
| Access to personal data by staff roles | Yes: who, what order, when, why (support ticket reference) |
| Administrative changes to platform and data stores | Yes |
| Order state changes with reason | Yes, part of the order timeline (REQ-13) |
| Alerts | Credential-stuffing patterns, rate-limit spikes, impossible-travel staff logins, unusual access volume, signing key or secret access failures |

Audit logs are append-only, access-controlled, kept for the retention period in C1 section 9.

## 8. Threat model for the checkout path (STRIDE, summary)

Scope: customer sign-in to confirmed order. Trust boundaries: internet to edge, edge to services, services to data, services to payment provider.

| # | Threat (STRIDE) | Example | Mitigation | Residual |
|---|---|---|---|---|
| T-1 | Spoofing | Attacker replays a stolen session | Short sessions, same-site HTTP-only cookies, token binding to session, anomaly alerts | Low |
| T-2 | Spoofing | Forged service call inside the cluster | Network policy, per-service tokens, validation in each service | Low |
| T-3 | Tampering | Customer changes price or quantity in the request | Server uses the basket and price list; client price is only a confirmation check (DD-5 in D4 section 5) | Low |
| T-4 | Tampering | Message altered or injected on the broker | Per-service broker permissions, TLS to broker, schema validation, consumers trust only expected exchanges | Low |
| T-5 | Repudiation | Customer or staff denies an action | Audit trail with correlation ids and the order timeline | Low |
| T-6 | Information disclosure | Personal data leaks through logs or events | Identifiers only, log scrubbing, restricted data class (REQ-20) | Low to medium |
| T-7 | Information disclosure | One customer reads another's order | Ownership check inside Ordering (BR-11), tests for object-level authorisation | Low |
| T-8 | Denial of service | Flood on checkout or login | WAF, rate limits, admission control (D4), autoscaling | Medium (large attacks need provider-level protection) |
| T-9 | Denial of service | Hoarding stock by repeated reservations without paying | Per-customer limits (BR-07), reservation expiry (BR-01), per-user checkout limiter | Medium |
| T-10 | Elevation of privilege | Customer calls an admin or any-order endpoint | Scope checks at gateway and service, admin APIs on separate routes, tested | Low |
| T-11 | Tampering | Duplicate or replayed payment callbacks | Idempotent handling keyed by provider reference, signature verification | Low |
| T-12 | Elevation of privilege | Compromised dependency or image | Scanning, signing, minimal images, least privilege | Medium |

Open item: a fuller threat model workshop with Security (S-05) before implementation.

## 9. Decisions in this document

| ID | Decision | Status |
|---|---|---|
| DS-1 | Gateway acts as BFF with cookie sessions and server-held tokens (ADR-008) | Proposed |
| DS-2 | In-cluster mutual TLS and service mesh are deferred. Revisit triggers: regulatory demand, multi-tenant workloads, or insider-threat requirements | Adopt (deferral) |
| DS-3 | Hosted card capture by the payment provider only | Adopt |
| DS-4 | Staff must use MFA | Adopt |

## Revision history

| Version | Date | Author | Change |
|---|---|---|---|
| 0.1 | 2026-10-24 | Lead Architect | First draft |
| 1.0 | 2026-10-24 | Lead Architect | Approved by sponsor |
