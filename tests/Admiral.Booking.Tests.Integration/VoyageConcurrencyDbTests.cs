using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AdmiralBooking.Tests.Integration;

public class VoyageConcurrencyDbTests
{
    [Fact]
    public async Task Two_independent_sessions_cannot_overbook_the_same_voyage()
    {
        var database = $"AdmiralBookingConcurrency_{Guid.NewGuid():N}";
        var connectionString =
            Environment.GetEnvironmentVariable("ADMIRAL_BOOKING_TEST_DB")
            ?? $"Server=(localdb)\\mssqllocaldb;Database={database};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<AppCommandDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        await using (var setup = new AppCommandDbContext(options))
        {
            await setup.Database.EnsureDeletedAsync();
            await setup.Database.MigrateAsync();

            var voyage = Voyage.Create(
                Guid.NewGuid(),
                LocationName.From("Rotterdam"),
                LocationName.From("Singapore"),
                DateTimeOffset.UtcNow.AddDays(5),
                ContainerUnits.From(2));
            setup.Voyages.Add(voyage);
            await setup.SaveChangesAsync();
        }

        await using var contextA = new AppCommandDbContext(options);
        await using var contextB = new AppCommandDbContext(options);
        var voyageA = await contextA.Voyages.Include(x => x.Reservations).SingleAsync();
        var voyageB = await contextB.Voyages.Include(x => x.Reservations).SingleAsync();

        voyageA.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(2));
        voyageB.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(1));

        var saveA = contextA.SaveChangesAsync();
        var saveB = contextB.SaveChangesAsync();
        var results = await Task.WhenAll(Wrap(saveA), Wrap(saveB));
        var errors = string.Join(" || ", results.Where(x => x.Exception is not null).Select(x => $"{x.Exception!.GetType().Name}: {x.Exception.Message}"));
        results.Count(x => x.Succeeded).Should().Be(1, because: errors);
        results.Count(x => x.Exception is DbUpdateConcurrencyException).Should().Be(1, because: errors);

        await using var verify = new AppCommandDbContext(options);
        var persisted = await verify.Voyages.Include(x => x.Reservations).SingleAsync();
        persisted.ReservedCapacity.Value.Should().BeLessThanOrEqualTo(persisted.TotalCapacity.Value);
        persisted.Reservations.Count.Should().Be(1);

        await using var cleanup = new AppCommandDbContext(options);
        await cleanup.Database.EnsureDeletedAsync();
    }

    private static async Task<(bool Succeeded, Exception? Exception)> Wrap(Task task)
    {
        try
        {
            await task;
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}
