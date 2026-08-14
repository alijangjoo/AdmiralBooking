namespace Admiral.Booking.AppCore.Domain.InboxMessages.Entities;

using Admiral.Booking.AppCore.Domain.Base;

public class InboxMessage : AggregateRoot<Guid>
{
    public string ConsumerName { get; private set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; private set; }

    private InboxMessage()
    {
    }

    public static InboxMessage Create(Guid messageId, string consumerName)
        => new()
        {
            Id = messageId,
            ConsumerName = consumerName,
            ProcessedAt = DateTimeOffset.UtcNow
        };
}
