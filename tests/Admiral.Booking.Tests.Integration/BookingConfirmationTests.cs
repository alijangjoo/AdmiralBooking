using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Admiral.Booking.AppCore.AppServices.OutboxMessages;
using Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Services;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdmiralBooking.Tests.Integration;

public class BookingConfirmationTests : IClassFixture<BookingApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
    private readonly BookingApiFactory _factory;
    private HttpClient _client = null!;

    public BookingConfirmationTests(BookingApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
        IdempotentBookingConfirmedConsumer.DownstreamEffectCount = 0;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Successful_booking_confirmation()
    {
        var seed = await SeedAsync();
        var existing = await ReadAsync(await _client.GetAsync($"/api/bookings/{seed.BookingAId}"));
        var response = await ConfirmAsync(seed.BookingAId, Guid.NewGuid().ToString());
        var payload = await ReadAsync(response);

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: $"get={existing} confirm={payload}");
        payload.GetProperty("success").GetBoolean().Should().BeTrue();
        payload.GetProperty("data").GetProperty("status").GetString().Should().Be("Confirmed");
        payload.GetProperty("data").GetProperty("reservationId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Equivalent_duplicate_request_is_idempotent()
    {
        var seed = await SeedAsync();
        var key = Guid.NewGuid().ToString();

        var first = await ConfirmAsync(seed.BookingAId, key);
        var second = await ConfirmAsync(seed.BookingAId, key);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var firstBody = await ReadAsync(first);
        var secondBody = await ReadAsync(second);
        secondBody.GetProperty("data").GetProperty("replayed").GetBoolean().Should().BeTrue();
        secondBody.GetProperty("data").GetProperty("reservationId").GetGuid()
            .Should().Be(firstBody.GetProperty("data").GetProperty("reservationId").GetGuid());

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
        (await db.CapacityReservations.CountAsync()).Should().Be(1);
        (await db.OutboxMessages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Same_idempotency_key_with_different_payload_is_rejected()
    {
        var seed = await SeedAsync();
        var key = Guid.NewGuid().ToString();
        var first = await ConfirmAsync(seed.BookingAId, key);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await ConfirmAsync(seed.BookingBId, key);
        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await ReadAsync(second);
        body.GetProperty("errors")[0].GetProperty("code").GetString().Should().Be("IDEMPOTENCY_KEY_CONFLICT");
    }

    [Fact]
    public async Task Closed_voyage_does_not_reserve_capacity()
    {
        var seed = await SeedAsync();
        var response = await ConfirmAsync(seed.ClosedVoyageBookingId, Guid.NewGuid().ToString());

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await ReadAsync(response);
        body.GetProperty("errors")[0].GetProperty("code").GetString().Should().Be("VOYAGE_NOT_OPEN");

        var booking = await _client.GetAsync($"/api/bookings/{seed.ClosedVoyageBookingId}");
        var bookingBody = await ReadAsync(booking);
        bookingBody.GetProperty("data").GetProperty("status").GetString().Should().Be("Draft");
    }

    [Fact]
    public async Task Optimistic_concurrency_conflict_is_handled_clearly()
    {
        var seed = await SeedAsync();
        var response = await ConfirmAsync(seed.BookingAId, Guid.NewGuid().ToString(), expectedVersion: 99);
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await ReadAsync(response);
        body.GetProperty("errors")[0].GetProperty("code").GetString().Should().Be("CONCURRENCY_CONFLICT");
    }

    [Fact]
    public async Task Duplicate_consumer_delivery_creates_one_downstream_effect()
    {
        var seed = await SeedAsync();
        var confirm = await ConfirmAsync(seed.BookingAId, Guid.NewGuid().ToString());
        confirm.EnsureSuccessStatusCode();
        var processed = await _client.PostAsync("/api/dev/outbox/process", null);
        processed.EnsureSuccessStatusCode();

        BookingConfirmedIntegrationEvent integrationEvent;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
            var outbox = await db.OutboxMessages.SingleAsync();
            integrationEvent = JsonSerializer.Deserialize<BookingConfirmedIntegrationEvent>(outbox.Payload, Json)
                ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");
            integrationEvent.EventId.Should().Be(outbox.Id);
        }

        IdempotentBookingConfirmedConsumer.DownstreamEffectCount.Should().Be(1);

        await RedeliverAsync(integrationEvent);
        await RedeliverAsync(integrationEvent);

        IdempotentBookingConfirmedConsumer.DownstreamEffectCount.Should().Be(1);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
            (await db.InboxMessages.CountAsync(x => x.Id == integrationEvent.EventId)).Should().Be(1);
        }
    }

    private async Task RedeliverAsync(BookingConfirmedIntegrationEvent integrationEvent)
    {
        using var scope = _factory.Services.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<IBookingConfirmedConsumer>();
        await consumer.ConsumeAsync(integrationEvent);
    }

    private async Task<SeedDto> SeedAsync(int voyageCapacity = 2)
    {
        var response = await _client.PostAsJsonAsync("/api/dev/seed", new { voyageCapacity });
        response.EnsureSuccessStatusCode();
        var body = await ReadAsync(response);
        var data = body.GetProperty("data");
        return new SeedDto(
            data.GetProperty("voyageId").GetGuid(),
            data.GetProperty("bookingAId").GetGuid(),
            data.GetProperty("bookingBId").GetGuid(),
            data.GetProperty("closedVoyageBookingId").GetGuid());
    }

    private Task<HttpResponseMessage> ConfirmAsync(Guid bookingId, string key, int? expectedVersion = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{bookingId}/confirm");
        request.Headers.Add("Idempotency-Key", key);
        request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());
        request.Content = JsonContent.Create(new { expectedVersion });
        return _client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadAsync(HttpResponseMessage response)
        => JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync(), Json);

    private sealed record SeedDto(Guid VoyageId, Guid BookingAId, Guid BookingBId, Guid ClosedVoyageBookingId);
}
