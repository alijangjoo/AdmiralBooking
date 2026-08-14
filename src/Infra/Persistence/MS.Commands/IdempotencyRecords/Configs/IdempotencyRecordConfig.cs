namespace Admiral.Booking.Infra.Persistence.MS.Commands.IdempotencyRecords.Configs;

using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class IdempotencyRecordConfig : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("IdempotencyRecords");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RequestFingerprint).HasMaxLength(128).IsRequired();
        builder.Property(x => x.BookingId).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.ResponsePayload);
        builder.Property(x => x.ErrorCode).HasMaxLength(64);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasDatabaseName("UX_IdempotencyRecords_Key");
    }
}
