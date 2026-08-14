using Admiral.Booking.Endpoints.Api.Workers;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdmiralBooking.Tests.Integration;

public class BookingApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"AdmiralBookingTests_{Guid.NewGuid():N}";

    public string ConnectionString { get; }

    public BookingApiFactory()
    {
        ConnectionString = Environment.GetEnvironmentVariable("ADMIRAL_BOOKING_TEST_DB")
            ?? $"Server=(localdb)\\mssqllocaldb;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CommandDb"] = ConnectionString,
                ["ConnectionStrings:QueryDb"] = ConnectionString,
                ["Outbox:SimulatePublishFailure"] = "false",
                ["Outbox:PublisherIntervalSeconds"] = "3600",
                ["Outbox:RetryDelaySeconds"] = "0"
            });
        });

        builder.ConfigureServices(services =>
        {
            foreach (var descriptor in services.Where(d => d.ImplementationType == typeof(OutboxPublisherHostedService)).ToList())
                services.Remove(descriptor);
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
                db.Database.EnsureDeleted();
            }
            catch
            {
                // LocalDB may already be gone in CI teardown.
            }
        }

        base.Dispose(disposing);
    }
}
