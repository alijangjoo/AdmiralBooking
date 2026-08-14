namespace Admiral.Booking.AppCore.Domain.Bookings.Enums;

public enum BookingStatus
{
    Draft = 0,
    PendingCapacity = 1,
    CapacityReserved = 2,
    Confirmed = 3,
    Rejected = 4,
    Cancelled = 5
}
