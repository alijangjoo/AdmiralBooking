namespace Admiral.Booking.Infra.Persistence.MS.Queries.Voyages.Configs;

using Admiral.Booking.Infra.Persistence.MS.Queries.Voyages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VoyageConfig : IEntityTypeConfiguration<Voyage>
{
    public void Configure(EntityTypeBuilder<Voyage> builder)
    {
        builder.ToTable("Voyages");
        builder.HasKey(x => x.Id);
    }
}
