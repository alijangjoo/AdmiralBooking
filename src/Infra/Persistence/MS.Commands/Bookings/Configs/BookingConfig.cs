namespace Admiral.Booking.Infra.Persistence.MS.Commands.Bookings.Configs;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookingConfig : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.VoyageId).IsRequired();
        builder.Property(x => x.RequestedUnits)
            .HasConversion(v => v.Value, v => ContainerUnits.From(v))
            .HasColumnName("RequestedUnits")
            .IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CapacityReservationId);
        builder.Property(x => x.ConfirmedAt);
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.VoyageId).HasDatabaseName("IX_Bookings_VoyageId");
    }
}
