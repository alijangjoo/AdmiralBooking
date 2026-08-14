namespace Admiral.Booking.AppCore.Domain.OutboxMessages.Enums;

public enum OutboxStatus
{
    Unknown = 0,
    Pending = 1,
    Published = 2,
    Failed = 3
}
