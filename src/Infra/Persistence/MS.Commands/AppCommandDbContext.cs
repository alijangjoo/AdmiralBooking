namespace Admiral.Booking.Infra.Persistence.MS.Commands;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;
using Admiral.Booking.AppCore.Domain.InboxMessages.Entities;
using Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class AppCommandDbContext : CommandDbContext
{
    public AppCommandDbContext(DbContextOptions<AppCommandDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Voyage> Voyages => Set<Voyage>();
    public DbSet<CapacityReservation> CapacityReservations => Set<CapacityReservation>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppCommandDbContext).Assembly);
    }

    public override int SaveChanges()
    {
        MarkNewClientGeneratedEntitiesAsAdded();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        MarkNewClientGeneratedEntitiesAsAdded();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void MarkNewClientGeneratedEntitiesAsAdded()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Unchanged or EntityState.Modified))
                continue;

            if (entry.Entity is not (CapacityReservation or OutboxMessage or IdempotencyRecord or InboxMessage))
                continue;

            var databaseValues = entry.GetDatabaseValues();
            if (databaseValues is null)
                entry.State = EntityState.Added;
        }
    }
}
