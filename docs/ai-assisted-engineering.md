# AI-Assisted Engineering

## Tools used

Cursor (Grok 4.6) against the local `Admiral.Booking` workspace.

## Main tasks delegated to AI

- Mapping the challenge document to a single confirmation / capacity-reservation vertical slice.
- Drafting aggregates, EF configurations, outbox/idempotency/inbox plumbing, automated tests, and documentation.
- Iterating Event Storming (happy path, failure paths, eight hotspots) and tightening the duplicate-delivery test so it redelivers the same `EventId`.

## Prompt strategy

1. Implement the challenge slice as a modular monolith with clear Booking and Voyage module boundaries; keep the public API to confirm + get + seed.
2. Follow-up constraints: one local transaction, Voyage-owned reservations, persisted HTTP idempotency distinct from status checks, genuine two-session concurrency test, at-least-once outbox with an idempotent consumer.

## How generated code and tests were validated

- Domain unit tests for cancelled/confirmed bookings, capacity overflow, closed voyage, missing reservation.
- Integration tests against SQL Server LocalDB: success, idempotent replay, key/payload conflict, concurrent HTTP confirmations, independent `DbContext` race, outbox failure + retry, and consumer redelivery of the same integration-event id.
- `dotnet build` / `dotnet test` after implementation and after later corrections.

## AI suggestion corrected or rejected

An early handler committed the idempotency `Started` row in a first transaction, then confirmed in a second. That made concurrency retries return `IDEMPOTENCY_IN_PROGRESS` forever. It was replaced with a single `SaveChanges` so a lost race stores nothing and retry remains safe.

A later generated “duplicate delivery” test only called the outbox processor twice. After the first run the row is `Published`, so the second call never reached the consumer. It was rewritten to confirm, publish once, then invoke the consumer twice with the same `EventId`.

## Design decision that remained human-owned

Keeping `CapacityReservation` as an entity inside the Voyage aggregate (not a separate aggregate or a network-split service) so the capacity invariant has one owner in this modular monolith.
