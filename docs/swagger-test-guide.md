# How to test the required scenarios in Swagger

Base URL in Development: `http://localhost:5279`

1. Run the API: `dotnet run --project src/Endpoints/Api`
2. Open [http://localhost:5279/swagger](http://localhost:5279/swagger)
3. Always start with **seed**. Copy the returned ids into later requests. There is no generic CRUD.

In Swagger, expand an operation → **Try it out**. For confirm, fill **path** `bookingId`, headers `Idempotency-Key` and `X-Correlation-ID`, then the JSON body.

Successful responses use this envelope:

```json
{
  "success": true,
  "data": { },
  "errors": [],
  "meta": { }
}
```

Business failures return HTTP `409` (or `400` for validation) with:

```json
{
  "success": false,
  "data": null,
  "errors": [
    {
      "message": "...",
      "code": "ERROR_CODE",
      "validationErrors": {}
    }
  ]
}
```

---

## 0. Seed sample data (required first)

**Swagger:** `POST /api/dev/seed`

**Payload**

```json
{
  "voyageCapacity": 2
}
```

**Sample response `data`**

```json
{
  "voyageId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "bookingAId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "bookingBId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
  "closedVoyageId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "closedVoyageBookingId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "voyageCapacity": 2,
  "bookingAUnits": 2,
  "bookingBUnits": 1
}
```

Meaning:

| Field | What it is |
| --- | --- |
| `bookingAId` | Draft booking that needs **all 2** remaining units |
| `bookingBId` | Draft booking that needs **1** unit on the same voyage |
| `closedVoyageBookingId` | Draft booking on a voyage already `ClosedForBooking` |

Keep these ids for every scenario below. Seed again if you need a clean voyage.

---

## 1. Successful confirmation (happy path)

**Swagger:** `POST /api/bookings/{bookingId}/confirm`

| | |
| --- | --- |
| Path | `bookingId` = `bookingAId` from seed |
| Header `Idempotency-Key` | `665886cf-e957-4555-b4ca-ad7b6baab8e5` |
| Header `X-Correlation-ID` | `f1382b1c-4525-4fd5-87cc-90803693a898` |

**Payload**

```json
{
  "expectedVersion": null
}
```

`expectedVersion` is the **Booking** version. `null` (or `{}`) skips that check.

**Expect:** HTTP `200`, `data.status` = `"Confirmed"`, `data.replayed` = `false`, non-empty `reservationId`.

Then **Swagger:** `GET /api/bookings/{bookingId}` with the same id. Status should stay `Confirmed`.

Covered by integration test `Successful_booking_confirmation`.

---

## Scenario A — Concurrent capacity reservation

Available capacity = 2. Booking A needs 2. Booking B needs 1. At most one incompatible reservation may succeed. `ReservedCapacity` must never exceed `TotalCapacity`.

**Swagger limitation:** Swagger UI cannot start two HTTP calls at the same instant. Two tabs executed quickly is a **manual approximation**. A sequential confirm of A then B will usually return `INSUFFICIENT_CAPACITY` for B after A already took both units. A genuine database race (`CONCURRENCY_CONFLICT`) needs overlapping `SaveChanges`.

**Manual approximation in Swagger**

1. Seed with `{ "voyageCapacity": 2 }`.
2. Open two browser tabs on Swagger.
3. Tab 1 — confirm `bookingAId` with its own key:

```
Idempotency-Key: 11111111-1111-1111-1111-111111111111
```

```json
{}
```

4. Tab 2 — confirm `bookingBId` with a **different** key:

```
Idempotency-Key: 22222222-2222-2222-2222-222222222222
```

```json
{}
```

5. Execute both as close together as you can.

**Expect:** one `200 Confirmed`. The other `409` with `CONCURRENCY_CONFLICT` or `INSUFFICIENT_CAPACITY`. GET both bookings: only one `Confirmed`.

**Real proof (not Swagger):**

| Test | Project | What it proves |
| --- | --- | --- |
| `Concurrent_capacity_reservations_preserve_capacity_invariant` | Integration | Two HTTP clients, `Task.WhenAll`, persisted capacity ≤ total |
| `Two_independent_sessions_cannot_overbook_the_same_voyage` | Integration | Two independent `DbContext` sessions, both `SaveChanges` started before either result is known |

```bash
dotnet test Admiral.Booking.slnx --filter FullyQualifiedName~Concurrent_capacity_reservations
dotnet test Admiral.Booking.slnx --filter FullyQualifiedName~Two_independent_sessions
```

---

## Scenario B — Duplicate confirmation request

Same confirmation sent more than once. Booking confirmed once, capacity reserved once, replay returns the stored result.

**Swagger:** confirm `bookingAId` twice with the **same** idempotency key.

First call:

```
Idempotency-Key: 665886cf-e957-4555-b4ca-ad7b6baab8e5
X-Correlation-ID: f1382b1c-4525-4fd5-87cc-90803693a898
```

```json
{
  "expectedVersion": null
}
```

Second call: **same path, same key, same body**.

**Expect:** both `200`. First: `replayed: false`. Second: `replayed: true` and the same `reservationId`.

Covered by integration test `Equivalent_duplicate_request_is_idempotent`.

Confirming an already-confirmed booking with a **new** key is a different rule (`BOOKING_ALREADY_CONFIRMED`). That is also visible in Swagger after a successful confirm:

```
Idempotency-Key: 99999999-9999-9999-9999-999999999999
```

```json
{}
```

**Expect:** `409`, `code`: `BOOKING_ALREADY_CONFIRMED`. Domain unit test `Confirmed_booking_cannot_be_confirmed_again` covers the aggregate without HTTP.

---

## Scenario C — Idempotency-key payload conflict

Same `Idempotency-Key`, non-equivalent request (different booking id). Original result must not change.

1. Confirm `bookingAId` (or seed a fresh pair if A is already confirmed from scenario B).
2. Call confirm on **`bookingBId`** with the **same** key.

```
POST /api/bookings/{bookingBId}/confirm
Idempotency-Key: 665886cf-e957-4555-b4ca-ad7b6baab8e5
```

```json
{
  "expectedVersion": null
}
```

**Expect:** `409`, `code`: `IDEMPOTENCY_KEY_CONFLICT`. GET `bookingAId` still `Confirmed`. GET `bookingBId` still `Draft`.

Covered by integration test `Same_idempotency_key_with_different_payload_is_rejected`.

Using the same key with a different `expectedVersion` on the **same** booking is also a fingerprint mismatch:

```json
{
  "expectedVersion": 99
}
```

**Expect:** `409`, `IDEMPOTENCY_KEY_CONFLICT` if that key already stored a result for `expectedVersion: null`.

---

## Scenario D — Voyage closed before reservation

**Swagger:** `POST /api/bookings/{closedVoyageBookingId}/confirm`

```
Idempotency-Key: 33333333-3333-3333-3333-333333333333
X-Correlation-ID: f1382b1c-4525-4fd5-87cc-90803693a898
```

```json
{}
```

**Expect:** `409`, `code`: `VOYAGE_NOT_OPEN` (business result, not a 500). GET that booking: still `Draft`. No capacity reserved.

Covered by integration test `Closed_voyage_does_not_reserve_capacity` and domain test `Closed_voyage_cannot_accept_a_reservation`.

---

## Scenario E — Event publication failure

Confirmation must commit even if publish fails. The outbox row stays pending and can be retried.

Swagger cannot toggle the failure gate by itself. Use configuration, then Swagger for confirm + outbox process.

1. In `src/Endpoints/Api/appsettings.Development.json` set:

```json
"Outbox": {
  "SimulatePublishFailure": true
}
```

2. Restart the API (the hosted publisher reads this setting).
3. Seed, then confirm `bookingAId` as in the happy path.
4. **Expect:** HTTP `200`, booking `Confirmed`.
5. **Swagger:** `POST /api/dev/outbox/process` (no body).
6. **Expect:** `{ "published": 0 }`. Console logs `Outbox publication failed`. Booking GET still `Confirmed`.
7. Set `SimulatePublishFailure` back to `false`, restart, call `POST /api/dev/outbox/process` again.
8. **Expect:** `{ "published": 1 }` (or the hosted publisher may publish within ~2 seconds after restart).

There is no GET-outbox endpoint. Pending vs published is visible in logs and in the integration test against `OutboxMessages`.

Covered by integration test `Event_publication_failure_leaves_an_outbox_record_and_retry_publishes_it` (`PublicationFailureGate`, no appsettings edit).

---

## Scenario F — Duplicate Integration Event delivery

Downstream must process the same `BookingConfirmed` `EventId` only once.

**Not testable in Swagger.** After a successful publish the outbox row is `Published`. Calling `POST /api/dev/outbox/process` a second time does **not** redeliver; it simply finds no pending row.

Redelivery is proven by calling the consumer twice with the same event id:

| Test | Project |
| --- | --- |
| `Duplicate_consumer_delivery_creates_one_downstream_effect` | Integration |

```bash
dotnet test Admiral.Booking.slnx --filter FullyQualifiedName~Duplicate_consumer_delivery
```

**Expect:** `DownstreamEffectCount == 1` and one `InboxMessages` row.

---

## Optimistic concurrency (stale Booking version)

**Swagger:** confirm a **Draft** booking (seed first) with a version that is not the current Booking version (seeded bookings start at `1`).

```
POST /api/bookings/{bookingAId}/confirm
Idempotency-Key: 44444444-4444-4444-4444-444444444444
```

```json
{
  "expectedVersion": 99
}
```

**Expect:** `409`, `code`: `CONCURRENCY_CONFLICT`. GET: still `Draft`.

This is the **Booking** `expectedVersion` check, not the Voyage last-seat race. Voyage occupancy under two writers is Scenario A / the two-session integration test.

Covered by integration test `Optimistic_concurrency_conflict_is_handled_clearly`.

---

## Domain rules that have no Swagger endpoint

These invariants are enforced on the aggregate. There is no Cancel API and no way to confirm while skipping reservation through HTTP.

| Rule | How it is tested |
| --- | --- |
| Requested capacity must be greater than zero | Unit: `Requested_capacity_must_be_greater_than_zero` |
| Cancelled booking cannot be confirmed | Unit: `Cancelled_booking_cannot_be_confirmed` |
| Confirmed booking cannot be confirmed again | Unit: `Confirmed_booking_cannot_be_confirmed_again` (HTTP: new idempotency key after confirm, see Scenario B) |
| Voyage cannot reserve beyond total capacity | Unit: `Voyage_cannot_reserve_beyond_total_capacity` |
| Closed / departed / cancelled voyage cannot accept a reservation | Unit: `Closed_voyage_cannot_accept_a_reservation`, `Departed_or_cancelled_voyage_cannot_accept_reservations` |
| Booking cannot confirm without a capacity reservation | Unit: `Booking_cannot_confirm_without_a_valid_capacity_reservation` |
| Same booking does not create a second active reservation | Unit: `Same_booking_does_not_create_a_second_active_reservation` |

```bash
dotnet test tests/Admiral.Booking.Tests.Unit
```

---

## Other endpoints

| Endpoint | In Swagger? | Notes |
| --- | --- | --- |
| `GET /api/bookings/{bookingId}` | Yes | No body. Use after every confirm. |
| `POST /api/dev/outbox/process` | Yes | No body. Response `{ "published": n }`. |
| `GET /health` | No | Open [http://localhost:5279/health](http://localhost:5279/health). Healthy → `Healthy`. |

---

## Run the automated suite

```bash
dotnet test Admiral.Booking.slnx
```

Stop the running API first if the build reports locked DLLs.

Same sample calls also live in [src/Endpoints/Api/Admiral.Booking.Endpoints.Api.http](../src/Endpoints/Api/Admiral.Booking.Endpoints.Api.http) for VS / REST Client (still not a true parallel race).
