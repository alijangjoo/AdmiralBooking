namespace Admiral.Booking.Infra.Persistence.MS.Queries.Bookings.Configs;

using Admiral.Booking.Infra.Persistence.MS.Queries.Bookings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BookingConfig : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasColumnName("Status");
    }
}
