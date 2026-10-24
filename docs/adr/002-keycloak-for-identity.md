# ADR-002: Keycloak as the identity provider

- **Status:** Accepted (confirmed in Phase D, see ADR-008)
- **Date:** 2026-10-24

## Context
We need OIDC / OAuth2: Authorization Code + PKCE for the React app, client credentials for service-to-service calls, JWT access tokens, refresh-token rotation, roles and scopes.

## Options considered
1. **Keycloak** - standard, open source, feature-complete, widely used in enterprises.
2. **Duende IdentityServer** - deep .NET integration and maximum learning, but commercial licence above a revenue threshold.
3. **OpenIddict** - free, flexible, but we would own user management, UI, and security upkeep.

## Decision
Use Keycloak. Identity is a solved problem; building our own identity server adds risk and effort without improving what this project teaches (token validation, authorization policies, edge security).

## Consequences
- (+) Faster to a secure baseline; realm export gives reproducible setup.
- (+) Interview story: "buy, don't build, security-critical infrastructure."
- (-) Another heavy container; claim-mapping quirks (roles live in nested claims).
- A thin custom identity service can be added later as an optional learning exercise without changing consumers, since they depend only on OIDC.
