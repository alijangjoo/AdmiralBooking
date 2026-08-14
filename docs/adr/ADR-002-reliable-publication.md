# ADR-002 Reliable BookingConfirmed Event Publication

## Context

A successful confirmation must eventually notify downstream systems. Publishing after `SaveChanges` can lose the message if the process crashes. Claiming end-to-end exactly-once would be false with at-least-once brokers.

## Why Outbox was selected

The confirmation transaction writes business state and an `OutboxMessages` row together. A background publisher reads `Pending` rows, publishes through `IIntegrationEventPublisher`, and marks `Published`. Transient failures increment attempts and remain visible in structured logs. `Outbox:SimulatePublishFailure` demonstrates the crash-after-commit path.

## Alternatives considered

- **Publish after commit**: simpler, loses events on crash.
- **Two-phase commit with a broker**: operationally heavy and out of scope.
- **Event sourcing**: not required for this slice.

## Delivery guarantee and duplicate-handling consequences

Guarantee is **at-least-once**. Consumers must be idempotent. This implementation stores `InboxMessages` keyed by integration-event id and ignores duplicates. Downstream effects are counted only for the first successful consume. Event contracts are versioned as `booking.confirmed.v1` and are independent of EF entities.
