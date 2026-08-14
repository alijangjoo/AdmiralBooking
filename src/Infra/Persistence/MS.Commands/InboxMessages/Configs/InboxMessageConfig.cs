namespace Admiral.Booking.Infra.Persistence.MS.Commands.InboxMessages.Configs;

using Admiral.Booking.AppCore.Domain.InboxMessages.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class InboxMessageConfig : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.ToTable("InboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ConsumerName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ProcessedAt).IsRequired();
        builder.HasIndex(x => new { x.Id, x.ConsumerName }).IsUnique().HasDatabaseName("UX_InboxMessages_Message_Consumer");
    }
}
