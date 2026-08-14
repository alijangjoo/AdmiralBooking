namespace Admiral.Booking.AppCore.Domain.Bookings.Enums;

public enum BookingStatus
{
    Unknown = 0,
    Draft = 1,
    PendingCapacity = 2,
    CapacityReserved = 3,
    Confirmed = 4,
    Rejected = 5,
    Cancelled = 6
}
