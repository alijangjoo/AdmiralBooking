# ADR-001 Voyage Capacity Concurrency Strategy

## Context

Two bookings may compete for the last voyage units. Application checks alone cannot prevent overbooking when two transactions read the same remaining capacity.

## Selected strategy

Optimistic concurrency on the Voyage aggregate using an explicit `Version` column (EF concurrency token), plus:

- Booking `Version` is also an EF concurrency token (same-booking races)
- Check constraint `ReservedCapacity <= TotalCapacity`
- Unique filtered index on active reservations `(BookingId, VoyageId)`

The Voyage aggregate remains the owner of the capacity invariant. `CapacityReservation` is an entity inside that aggregate. Optional HTTP `expectedVersion` is the Booking version; it does not protect Voyage occupancy.

## Alternatives considered

- **Pessimistic `UPDLOCK`**: stronger serialization, higher blocking, harder to test, premature for this load.
- **Separate reservation aggregate**: better for future service extraction, but confirmation would need a saga or two-phase commit.
- **In-memory / Redis lock**: does not protect other app instances or direct SQL writers.

## Consequences and failure behaviour

The first `SaveChanges` wins. The loser receives `CONCURRENCY_CONFLICT` and may retry. Retry is safe: the failed transaction did not confirm the booking or store an idempotency result. If capacity is gone after retry, the domain returns `INSUFFICIENT_CAPACITY`. The check constraint is the last line of defence if application logic is bypassed.
