namespace Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;

using Admiral.Booking.AppCore.Domain.Base;
using Admiral.Booking.AppCore.Domain.OutboxMessages.Enums;

public class OutboxMessage : AggregateRoot<Guid>
{
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public Guid DeduplicationKey { get; private set; }
    public OutboxStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }

    private OutboxMessage()
    {
    }

    public static OutboxMessage Create(Guid id, string eventType, string payload, Guid deduplicationKey)
        => new()
        {
            Id = id,
            EventType = eventType,
            Payload = payload,
            DeduplicationKey = deduplicationKey,
            Status = OutboxStatus.Pending,
            AttemptCount = 0,
            OccurredAt = DateTimeOffset.UtcNow,
            NextAttemptAt = DateTimeOffset.UtcNow
        };

    public void MarkPublished()
    {
        Status = OutboxStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
        LastError = null;
        NextAttemptAt = null;
    }

    public void MarkFailed(string error, TimeSpan retryDelay, int maxAttempts)
    {
        AttemptCount++;
        LastError = error.Length > 2000 ? error[..2000] : error;
        NextAttemptAt = DateTimeOffset.UtcNow.Add(retryDelay);
        Status = AttemptCount >= maxAttempts ? OutboxStatus.Failed : OutboxStatus.Pending;
    }
}
