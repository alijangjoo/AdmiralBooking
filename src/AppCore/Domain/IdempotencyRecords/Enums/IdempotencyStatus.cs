namespace Admiral.Booking.AppCore.Domain.IdempotencyRecords.Enums;

public enum IdempotencyStatus
{
    Unknown = 0,
    Started = 1,
    Completed = 2,
    Failed = 3
}
