namespace Admiral.Booking.Infra.Persistence.MS.Commands.OutboxMessages.Configs;

using Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Payload).IsRequired();
        builder.Property(x => x.DeduplicationKey).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>().IsRequired();
        builder.Property(x => x.AttemptCount).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.HasIndex(x => x.DeduplicationKey).IsUnique().HasDatabaseName("UX_OutboxMessages_DeduplicationKey");
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt }).HasDatabaseName("IX_OutboxMessages_Pending");
    }
}
