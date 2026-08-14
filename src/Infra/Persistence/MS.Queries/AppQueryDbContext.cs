namespace Admiral.Booking.Infra.Persistence.MS.Queries;

using Admiral.Booking.Infra.Persistence.Base.Queries;
using Admiral.Booking.Infra.Persistence.MS.Queries.Bookings.Entities;
using Admiral.Booking.Infra.Persistence.MS.Queries.Voyages.Entities;
using Microsoft.EntityFrameworkCore;

public class AppQueryDbContext : QueryDbContext
{
    public AppQueryDbContext(DbContextOptions<AppQueryDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Voyage> Voyages => Set<Voyage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppQueryDbContext).Assembly);
    }
}
