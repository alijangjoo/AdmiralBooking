# AI-Assisted Engineering

## Tools used

Cursor (Grok 4.6) against the local `Admiral.Booking` workspace, with `C:\Users\jangjoo.ali\source\repos\dizi` as the structural reference.

## Main tasks delegated to AI

- Extracting the challenge document and mapping it to a vertical slice.
- Copying Dizi layering (`AppCore`, `Shared`, `AppServices`, `DomainServices`, `Infra.Persistence`, `Endpoints.Base`).
- Generating aggregates, EF configs, outbox/idempotency plumbing, tests, and documentation drafts.

## Prompt strategy

1. “Implement everything in the challenge doc; follow Dizi `src` naming and CQRS folder layout.”
2. Follow-up constraints: one local transaction, Voyage-owned reservations, persisted idempotency distinct from status checks, genuine two-session concurrency test.

## How generated code and tests were validated

- Domain unit tests for cancelled/confirmed bookings, capacity overflow, closed voyage, missing reservation.
- Integration tests against SQL Server LocalDB: success, idempotent replay, key/payload conflict, concurrent HTTP confirmations, independent `DbContext` race, outbox failure + retry, inbox dedup.
- `dotnet build` / `dotnet test` after implementation.

## AI suggestion corrected or rejected

An early handler committed the idempotency `Started` row in a first transaction, then confirmed in a second. That made concurrency retries return `IDEMPOTENCY_IN_PROGRESS` forever. It was replaced with a single `SaveChanges` so a lost race stores nothing and retry remains safe.

## Design decision that remained human-owned

Keeping `CapacityReservation` as an entity inside the Voyage aggregate (not a separate aggregate or a microservice) so the capacity invariant has one owner in this modular monolith.
