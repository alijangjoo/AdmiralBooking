# Admiral.Booking

Senior backend challenge slice: **shipment booking confirmation** and **voyage capacity reservation**.

The solution is a .NET 8 modular monolith: `AppCore` (Domain, Shared, AppServices, DomainServices), `Infra.Persistence` (Base, MS.Commands, MS.Queries), and `Endpoints` (Base, Api). Two modules share one deployable and one SQL Server database: Booking Management and Voyage & Capacity Management.

## Challenge overview

Confirm an existing Booking only when the Voyage is open and has remaining container capacity. The implementation must stay correct under HTTP retries, concurrent last-seat reservations, lost publication after commit, and duplicate consumer delivery.

## Documentation

| Document | Path |
| --- | --- |
| Architecture note | [docs/architecture.md](docs/architecture.md) |
| Ubiquitous language | [docs/ubiquitous-language.md](docs/ubiquitous-language.md) |
| Event Storming | [docs/diagrams/event-storming.md](docs/diagrams/event-storming.md) |
| Component / domain view | [docs/diagrams/architecture.md](docs/diagrams/architecture.md) |
| Confirmation sequence | [docs/diagrams/booking-confirmation-sequence.md](docs/diagrams/booking-confirmation-sequence.md) |
| ADR-001 Capacity concurrency | [docs/adr/ADR-001-capacity-concurrency.md](docs/adr/ADR-001-capacity-concurrency.md) |
| ADR-002 Reliable publication | [docs/adr/ADR-002-reliable-publication.md](docs/adr/ADR-002-reliable-publication.md) |
| AI-assisted engineering | [docs/ai-assisted-engineering.md](docs/ai-assisted-engineering.md) |
| Swagger test guide | [docs/swagger-test-guide.md](docs/swagger-test-guide.md) |
| Sample requests | [src/Endpoints/Api/Admiral.Booking.Endpoints.Api.http](src/Endpoints/Api/Admiral.Booking.Endpoints.Api.http) |

## Assumptions

- Bookings and voyages are created through `POST /api/dev/seed` (no generic CRUD).
- Command and query databases may be the same SQL Server database.
- Integration events are published in-process (no Kafka).
- Authentication, pricing, and shipment execution are out of scope.

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (default) or SQL Server 2022 (`docker compose up sqlserver`)

## How to build

```bash
dotnet build Admiral.Booking.slnx
```

## How to run

```bash
dotnet run --project src/Endpoints/Api
```

Swagger: `http://localhost:5279/swagger`. Health: `GET /health`.

## How to run tests

```bash
dotnet test Admiral.Booking.slnx
```

Integration tests create disposable LocalDB databases. Override with `ADMIRAL_BOOKING_TEST_DB` if needed.

## Database setup and migrations

Development startup runs `Database.Migrate()`. To generate or apply migrations manually:

```bash
dotnet ef migrations add InitialCreate --project src/Infra/Persistence/MS.Commands --startup-project src/Endpoints/Api
dotnet ef database update --project src/Infra/Persistence/MS.Commands --startup-project src/Endpoints/Api
```

Hand-written DDL also lives under `src/Infra/Persistence/MS.Commands/**/Scripts/`.

Default connection (LocalDB):

`Server=(localdb)\mssqllocaldb;Database=AdmiralBooking;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true`

Docker SQL Server:

```bash
docker compose up -d
```

Then set `ConnectionStrings:CommandDb` / `QueryDb` to `Server=localhost,1433;Database=AdmiralBooking;User Id=sa;Password=Your_password123;TrustServerCertificate=True`.

## How to initialise sample data

```http
POST /api/dev/seed
{ "voyageCapacity": 2 }
```

Returns `voyageId`, `bookingAId` (needs all remaining units), `bookingBId` (needs 1 unit), and a closed-voyage booking.

## Sample requests

Use [Admiral.Booking.Endpoints.Api.http](src/Endpoints/Api/Admiral.Booking.Endpoints.Api.http) after replacing seed ids. Required headers:

```http
POST /api/bookings/{bookingId}/confirm
Idempotency-Key: 665886cf-e957-4555-b4ca-ad7b6baab8e5
X-Correlation-ID: f1382b1c-4525-4fd5-87cc-90803693a898
```

| Scenario | How |
| --- | --- |
| A — Concurrent last-seat | Confirm `bookingAId` and `bookingBId` in parallel, different keys |
| B — Duplicate confirmation | Replay the same key on the same booking |
| C — Idempotency payload conflict | Reuse the same key on a different booking |
| D — Voyage closed | Confirm `closedVoyageBookingId` |
| E — Publication failure | Set `Outbox:SimulatePublishFailure` then process outbox |
| F — Duplicate event delivery | Automated: same `EventId` delivered twice to the consumer |

## How to reproduce the concurrency scenario

1. Seed with `voyageCapacity: 2`.
2. Confirm `bookingAId` and `bookingBId` in parallel, each with its own `Idempotency-Key`.
3. One request succeeds; the other returns `CONCURRENCY_CONFLICT` or `INSUFFICIENT_CAPACITY`.
4. Persisted `ReservedCapacity` never exceeds `TotalCapacity`.

Automated coverage: `ConcurrentReservationTests` and `VoyageConcurrencyDbTests` (two independent database sessions started before either `SaveChanges` completes).

## How to simulate event-publication failure

Set `Outbox:SimulatePublishFailure` to `true`, confirm a booking, then `POST /api/dev/outbox/process`. The booking stays `Confirmed` and the outbox row stays `Pending`. Set the flag back to `false` (or use the test `PublicationFailureGate`) and process again to publish.

## How to reproduce duplicate Integration Event delivery

Confirm a booking and process the outbox once (first delivery). The automated test then calls the consumer again with the same `EventId`. Inbox dedup keeps `DownstreamEffectCount` at 1. Processing a already-published outbox row a second time is not a redelivery; at-least-once brokers redeliver the same event id.

## Known limitations

- Idempotency keys are not expired.
- Outbox publisher is in-process.
- No bounded automatic retry on `CONCURRENCY_CONFLICT` (clients retry).

## What I would improve with additional time

- Bounded confirmation retry on concurrency conflicts.
- Outbox metrics (backlog, publish latency) and tracing.
- Idempotency TTL and a proper broker adapter.
- If Voyage becomes a separate service: replace the local transaction with an explicit reservation API and a compensation policy.
