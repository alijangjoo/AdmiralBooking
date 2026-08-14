# Ubiquitous Language

| Term | Meaning |
| --- | --- |
| Booking | A customer transportation request for a number of container units on one Voyage. |
| Voyage | A scheduled journey with origin, destination, departure date, and a finite container capacity. |
| Container unit | The capacity unit reserved and consumed by a Booking. Must be greater than zero. |
| Capacity reservation | Voyage-owned record that capacity was set aside for one Booking. Active, Released, or Consumed. |
| Open for booking | Voyage state that still accepts reservations. |
| Confirm booking | The command that validates eligibility, reserves capacity, and marks the Booking Confirmed. |
| Idempotency key | Client-generated key plus a fingerprint of equivalent content (`bookingId` + optional `expectedVersion`) so retries replay and mismatched payloads conflict. |
| Domain event | In-process fact raised by an aggregate (`CapacityReserved`, `BookingConfirmed`). |
| Integration event | Versioned contract published outside the module (`booking.confirmed.v1`). |
| Outbox | Durable table written in the same transaction as confirmation, later published at-least-once. |
| Inbox | Consumer-side record that prevents duplicate downstream effects after redelivery. |
| Concurrency conflict | Two writers loaded the same Voyage version; the loser must retry. |
