namespace Admiral.Booking.AppCore.Domain.IdempotencyRecords.Enums;

public enum IdempotencyStatus
{
    Started = 0,
    Completed = 1,
    Failed = 2
}
