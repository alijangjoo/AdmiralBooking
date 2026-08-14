namespace Admiral.Booking.AppCore.Shared.Bookings.Commands.Confirm;

public class ConfirmBookingCommandResult
{
    public Guid BookingId { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid VoyageId { get; set; }
    public Guid? ReservationId { get; set; }
    public int RequestedUnits { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int Version { get; set; }
    public bool Replayed { get; set; }
}
