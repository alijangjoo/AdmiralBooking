namespace Admiral.Booking.Infra.Persistence.MS.Queries.Bookings.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid VoyageId { get; set; }
    public int RequestedUnits { get; set; }
    public int Status { get; set; }
    public Guid? CapacityReservationId { get; set; }
    public DateTimeOffset? ConfirmedAt { get; set; }
    public int Version { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
