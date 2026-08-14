namespace Admiral.Booking.AppCore.Domain.Voyages.Enums;

public enum VoyageStatus
{
    Unknown = 0,
    OpenForBooking = 1,
    ClosedForBooking = 2,
    Departed = 3,
    Cancelled = 4
}
