using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Admiral.Booking.AppCore.Domain.Bookings.Enums;
using Admiral.Booking.AppCore.Domain.OutboxMessages.Enums;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Services;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AdmiralBooking.Tests.Integration;

public class ConcurrentReservationTests
{
    [Fact]
    public async Task Concurrent_capacity_reservations_preserve_capacity_invariant()
    {
        await using var factory = new BookingApiFactory();
        await factory.ResetDatabaseAsync();
        var clientA = factory.CreateClient();
        var clientB = factory.CreateClient();

        var seedResponse = await clientA.PostAsJsonAsync("/api/dev/seed", new { voyageCapacity = 2 });
        seedResponse.EnsureSuccessStatusCode();
        var seed = JsonSerializer.Deserialize<JsonElement>(
            await seedResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var data = seed.GetProperty("data");
        var bookingA = data.GetProperty("bookingAId").GetGuid();
        var bookingB = data.GetProperty("bookingBId").GetGuid();
        var voyageId = data.GetProperty("voyageId").GetGuid();

        var taskA = Confirm(clientA, bookingA);
        var taskB = Confirm(clientB, bookingB);
        await Task.WhenAll(taskA, taskB);

        var results = new[] { taskA.Result, taskB.Result };
        var details = string.Join(" || ", results.Select(x => $"{(int)x.StatusCode}:{x.Content.ReadAsStringAsync().GetAwaiter().GetResult()}"));
        results.Count(x => x.IsSuccessStatusCode).Should().Be(1, because: details);
        results.Count(x => x.StatusCode is HttpStatusCode.Conflict).Should().Be(1, because: details);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
        var voyage = await db.Voyages.Include(x => x.Reservations).SingleAsync(x => x.Id == voyageId);
        voyage.ReservedCapacity.Value.Should().BeLessThanOrEqualTo(voyage.TotalCapacity.Value);
        voyage.Reservations.Count(x =>
            x.Status is CapacityReservationStatus.Active or CapacityReservationStatus.Consumed).Should().Be(1);
        (await db.Bookings.CountAsync(x => x.Status == BookingStatus.Confirmed)).Should().Be(1);
    }

    private static Task<HttpResponseMessage> Confirm(HttpClient client, Guid bookingId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{bookingId}/confirm");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        request.Content = JsonContent.Create(new { });
        return client.SendAsync(request);
    }
}

public class OutboxFailureTests
{
    [Fact]
    public async Task Event_publication_failure_leaves_an_outbox_record_and_retry_publishes_it()
    {
        await using var factory = new BookingApiFactory();
        await factory.ResetDatabaseAsync();
        var client = factory.CreateClient();
        var gate = factory.Services.GetRequiredService<PublicationFailureGate>();
        gate.Fail = true;

        var seedResponse = await client.PostAsJsonAsync("/api/dev/seed", new { voyageCapacity = 2 });
        seedResponse.EnsureSuccessStatusCode();
        var seed = JsonSerializer.Deserialize<JsonElement>(
            await seedResponse.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var bookingId = seed.GetProperty("data").GetProperty("bookingAId").GetGuid();

        var confirm = await Confirm(client, bookingId);
        confirm.IsSuccessStatusCode.Should().BeTrue();
        await client.PostAsync("/api/dev/outbox/process", null);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
            var booking = await db.Bookings.SingleAsync(x => x.Id == bookingId);
            booking.Status.Should().Be(BookingStatus.Confirmed);
            var outbox = await db.OutboxMessages.SingleAsync();
            outbox.Status.Should().Be(OutboxStatus.Pending);
            outbox.AttemptCount.Should().BeGreaterThan(0);
        }

        gate.Fail = false;
        await Task.Delay(50);
        await client.PostAsync("/api/dev/outbox/process", null);

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
            var outbox = await db.OutboxMessages.SingleAsync();
            outbox.Status.Should().Be(OutboxStatus.Published);
            (await db.InboxMessages.CountAsync()).Should().Be(1);
        }
    }

    private static Task<HttpResponseMessage> Confirm(HttpClient client, Guid bookingId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/bookings/{bookingId}/confirm");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        request.Content = JsonContent.Create(new { });
        return client.SendAsync(request);
    }
}
