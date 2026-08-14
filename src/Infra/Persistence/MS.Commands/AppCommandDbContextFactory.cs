using Admiral.Booking.Infra.Persistence.MS.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Admiral.Booking.Infra.Persistence.MS.Commands;

public class AppCommandDbContextFactory : IDesignTimeDbContextFactory<AppCommandDbContext>
{
    public AppCommandDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ADMIRAL_BOOKING_COMMAND_DB")
            ?? "Server=(localdb)\\mssqllocaldb;Database=AdmiralBooking;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<AppCommandDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppCommandDbContext(options);
    }
}
