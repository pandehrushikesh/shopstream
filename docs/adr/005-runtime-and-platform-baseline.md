# ADR-005: Runtime and platform baseline (.NET 10 LTS, containers, Kubernetes, cloud-neutral)

- **Status:** Accepted (sponsor approval, 2026-10-24)
- **Date:** 2026-10-24
- **Reference:** `Project_Documentation/D-Technology-Architecture/D1`, `D2`

## Context
.NET 8 and .NET 9 reach end of support on 10 November 2026. .NET 10 is LTS, supported through November 2028. The sponsor requires a cloud-neutral technology architecture.

## Decision
- Target **.NET 10** for all new and migrated code.
- Package as OCI containers and run on **Kubernetes**. Use managed services only where they speak an open protocol and have a documented exit path.
- Keep infrastructure as code (OpenTofu/Terraform) with a thin provider layer.

## Consequences
- (+) Supported runtime for the life of the first releases. Portable across providers.
- (-) The existing Catalogue spike (net8.0) must be retargeted to net10.0 before further development.
- (-) Kubernetes skills are required in the platform team (risk R-17).
