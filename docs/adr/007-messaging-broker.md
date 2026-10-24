# ADR-007: RabbitMQ as the message broker; messaging library left open

- **Status:** Accepted. Broker: RabbitMQ. Library: MassTransit v8 first, free alternative as fallback (sponsor decision, 2026-10-24; OD-1 closed with an accepted risk)
- **Date:** 2026-10-24
- **Reference:** `Project_Documentation/D-Technology-Architecture/D2 section 5`, `D4 section 2.4`

## Context
Checkout needs reliable commands and events, dead-lettering and per-consumer queues at about 3,000 to 4,000 messages per second at burst. MassTransit v9 is a commercial product. MassTransit v8 remains open source but its reported security-patch window ends in 2026. The sponsor has ruled out a paid licence (fictional, non-commercial project).

## Decision
- Use **RabbitMQ** with quorum queues, one queue per consumer.
- Application code depends on our own messaging port, not on a vendor library.
- Use **MassTransit v8** (open source) for outbox, inbox and the Ordering saga. No paid licence.
- If v8 proves painful (.NET 10 compatibility, saga fit, missing patches), switch to a free alternative: another open-source .NET library or a thin in-house layer on the RabbitMQ client. A Phase E spike confirms v8 on .NET 10 first and sets the switch criteria.
- The messaging port stays, so the switch affects only the infrastructure layer.

## Consequences
- (+) Meets the volume with headroom (to be proven by load test). Log-based broker (Kafka) stays a scale-out option.
- (-) **Accepted risk:** v8 receives no further security patches after the reported 2026 window. Mitigation: port isolation, spike, review at each .NET LTS move.
- (-) Outbox, inbox and saga behaviour must be proven by tests whichever way the library question goes (risk R-12).
