namespace Admiral.Booking.Infra.Persistence.MS.Commands.Voyages.Configs;

using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VoyageConfig : IEntityTypeConfiguration<Voyage>
{
    public void Configure(EntityTypeBuilder<Voyage> builder)
    {
        builder.ToTable("Voyages", table =>
        {
            table.HasCheckConstraint("CK_Voyages_ReservedCapacity", "[ReservedCapacity] <= [TotalCapacity]");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Origin)
            .HasConversion(v => v.Value, v => LocationName.From(v))
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(x => x.Destination)
            .HasConversion(v => v.Value, v => LocationName.From(v))
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(x => x.DepartureDate).IsRequired();
        builder.Property(x => x.TotalCapacity)
            .HasConversion(v => v.Value, v => ContainerUnits.From(v))
            .IsRequired();
        builder.Property(x => x.ReservedCapacity)
            .HasConversion(v => v.Value, v => v == 0 ? ContainerUnits.Zero : ContainerUnits.From(v))
            .IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken().IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasMany(x => x.Reservations)
            .WithOne()
            .HasForeignKey(x => x.VoyageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Reservations)
            .HasField("_reservations")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.RemainingCapacity);
    }
}
