namespace Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;

public class GetBookingByIdQueryResult
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VoyageId { get; set; }
    public int RequestedUnits { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? CapacityReservationId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
