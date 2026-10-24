# ADR-008: Browser authentication through a backend-for-frontend in the gateway

- **Status:** Accepted (sponsor approval, 2026-10-24)
- **Date:** 2026-10-24
- **Reference:** `Project_Documentation/D-Technology-Architecture/D3`

## Context
The storefront is a single-page application. Tokens stored in browser JavaScript can be stolen through script injection.

## Decision
The gateway performs the OpenID Connect authorization code flow with PKCE, keeps tokens server-side, and gives the browser only a secure, HTTP-only, same-site session cookie. Session state is kept in the shared Redis-compatible store. Non-browser clients use bearer tokens validated by the same gateway. Every service still validates tokens and enforces scopes itself.

## Consequences
- (+) Tokens never reach browser code. Smaller attack surface.
- (-) The gateway becomes stateful through its session store and needs CSRF protection. A sign-in outage affects only new sessions.
- Keycloak is confirmed as the identity provider; ADR-002 moves to Accepted.
