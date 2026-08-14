namespace Admiral.Booking.Infra.Persistence.MS.Commands.Voyages.Configs;

using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CapacityReservationConfig : IEntityTypeConfiguration<CapacityReservation>
{
    public void Configure(EntityTypeBuilder<CapacityReservation> builder)
    {
        builder.ToTable("CapacityReservations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.VoyageId).IsRequired();
        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.ReservedUnits)
            .HasConversion(v => v.Value, v => ContainerUnits.From(v))
            .IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.BookingId, x.VoyageId })
            .IsUnique()
            .HasFilter("[Status] = 0")
            .HasDatabaseName("UX_CapacityReservations_Booking_Voyage_Active");
    }
}
