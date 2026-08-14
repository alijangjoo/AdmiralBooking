namespace Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;

using Admiral.Booking.AppCore.Domain.Base;
using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Enums;

public class IdempotencyRecord : AggregateRoot<Guid>
{
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string RequestFingerprint { get; private set; } = string.Empty;
    public Guid BookingId { get; private set; }
    public IdempotencyStatus Status { get; private set; }
    public string? ResponsePayload { get; private set; }
    public string? ErrorCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private IdempotencyRecord()
    {
    }

    public static IdempotencyRecord Start(Guid id, string idempotencyKey, string requestFingerprint, Guid bookingId)
        => new()
        {
            Id = id,
            IdempotencyKey = idempotencyKey,
            RequestFingerprint = requestFingerprint,
            BookingId = bookingId,
            Status = IdempotencyStatus.Started,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public bool HasSameFingerprint(string fingerprint)
        => string.Equals(RequestFingerprint, fingerprint, StringComparison.Ordinal);

    public void Complete(string responsePayload)
    {
        Status = IdempotencyStatus.Completed;
        ResponsePayload = responsePayload;
        ErrorCode = null;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorCode, string responsePayload)
    {
        Status = IdempotencyStatus.Failed;
        ErrorCode = errorCode;
        ResponsePayload = responsePayload;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
