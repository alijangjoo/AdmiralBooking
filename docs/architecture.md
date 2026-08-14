# Architecture Note

This service is a modular monolith vertical slice for **booking confirmation** and **voyage capacity reservation**. It is one deployable ASP.NET Core application with two modules that share a SQL Server database and a single local transaction for confirmation.

These terms are not interchangeable: **Bounded Context** is a language boundary (Booking Management, Voyage & Capacity); **module** is a folder/aggregate grouping inside one process; **aggregate** is the consistency boundary; **entity** is identity inside an aggregate; **database boundary** is one SQL Server database; **deployment boundary** is one ASP.NET process.

## Domain understanding and assumptions

A customer already has a Draft `Booking` that references a `Voyage` and a number of container units. Confirmation is allowed only when the booking is eligible, the voyage is `OpenForBooking`, and remaining capacity is sufficient. The same HTTP request may be retried. Two customers may compete for the last units.

Assumptions: no pricing, payments, auth, or shipment execution; sample data is created through `POST /api/dev/seed`; Command and Query connection strings may point at the same database; publication uses an in-process publisher rather than Kafka.

## Tactical DDD map

| Concept | In this slice |
| --- | --- |
| Bounded contexts | Booking Management; Voyage & Capacity Management (surrounding Customer, Pricing, Notification, Shipment Execution are not implemented) |
| Aggregates | `Booking`; `Voyage` |
| Entities | `CapacityReservation` (inside Voyage, not its own aggregate) |
| Value objects | `ContainerUnits`, `LocationName` |
| Domain service | `BookingConfirmationService` coordinates the two aggregates without owning either invariant |
| Domain events | `CapacityReserved`, `CapacityReservationRejected`, `BookingConfirmed`, and other in-process facts on the aggregate |
| Integration event | `booking.confirmed.v1` (`BookingConfirmedIntegrationEvent`) |
| Repositories | `IBookingCommandRepository`, `IVoyageCommandRepository`, plus outbox / idempotency / inbox persistence |
| Transaction boundary | One EF `SaveChanges`: Booking + Voyage + Outbox + Idempotency record |
| Concurrency boundary | Voyage row (`Version` token). Booking `Version` is also a token so two writers cannot silently overwrite the same booking |

## Aggregates and invariants

**Booking** owns confirmation lifecycle: Draft → PendingCapacity → CapacityReserved → Confirmed. Cancelled and already-confirmed bookings cannot be confirmed. Confirmation requires an attached reservation id. Invalid transitions throw from the aggregate, not from controllers.

**Voyage** owns the capacity invariant `ReservedCapacity <= TotalCapacity`. `CapacityReservation` is an entity inside the Voyage aggregate. That keeps the contested resource and its reservation list in one consistency boundary. Trade-off: more row contention on Voyage, simpler correctness, and a clear extraction path later (Voyage service would then expose a reservation API and this module would use a saga). A second `ReserveCapacity` for the same booking returns the existing active reservation; the filtered unique index is the database guarantee.

## Transaction boundaries

Confirmation commits **Booking + Voyage + Outbox + Idempotency record** in one EF Core `SaveChanges` (one local SQL transaction). That is appropriate for a modular monolith.

Domain events (`CapacityReserved`, `CapacityReservationRejected`, `BookingConfirmed`, and other aggregate facts) are raised on the aggregate and stay in-process. There is no broker dispatch for them. Only `BookingConfirmed` is projected into the versioned integration contract `booking.confirmed.v1` and stored in the outbox in the same commit. That contract is independent of EF entities and is the only event that crosses a module or Bounded Context boundary.

## Concurrency strategy

The contested resource is the Voyage row. Optimistic concurrency uses the Voyage `Version` column as an EF concurrency token. Optional request `expectedVersion` is the **Booking** version (client-side stale-write check); it is not a substitute for Voyage occupancy control. The losing `SaveChanges` raises `DbUpdateConcurrencyException` and returns `CONCURRENCY_CONFLICT`. Retry with the same idempotency key is safe because a failed transaction does not store an idempotency result. Final protection: check constraint `CK_Voyages_ReservedCapacity` and unique filtered index `UX_CapacityReservations_Booking_Voyage_Active`. In-memory locks are unnecessary; they would not survive multiple process instances.

## Idempotency strategy

Scope: `POST /api/bookings/{bookingId}/confirm` plus `Idempotency-Key`. Fingerprint is SHA-256 of `bookingId|expectedVersion`. A unique index on the key is the database guarantee. Same key + same fingerprint returns the stored result (`replayed: true`). Same key + different fingerprint returns `IDEMPOTENCY_KEY_CONFLICT`. This is distinct from `Booking.Status == Confirmed` and from inbox deduplication of broker redelivery.

## Outbox design

After a successful confirmation the integration event is inserted as `Pending`. A hosted publisher (and `POST /api/dev/outbox/process`) publishes at-least-once. Failures stay durable; `Outbox:SimulatePublishFailure` forces a failure for demos. Consumers record `InboxMessages` by event id so duplicate delivery has one downstream effect. This is not end-to-end exactly-once.

## Observability and production metrics

Structured logs include correlation id, Booking id, Voyage id, concurrency conflicts, and outbox publish attempts. `/health` checks the command database. Metrics described rather than fully deployed, and the ones that matter in production are: confirmation success/failure rate, capacity-rejection count (`INSUFFICIENT_CAPACITY`), concurrency-conflict count, Outbox backlog (Pending rows) and publish latency, and confirmation latency from request to commit.

## Trade-offs and limitations

- Two aggregates in one transaction couples Booking Management and Voyage & Capacity until a later service split.
- Optimistic concurrency can starve a client under extreme contention; a bounded retry could be added.
- The publisher is in-process; swapping it for a broker is a publisher-adapter change.
- Idempotency records are not expired.
