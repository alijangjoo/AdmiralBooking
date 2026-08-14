namespace Admiral.Booking.AppCore.Shared.Bookings.Commands.Seed;

public class SeedSampleDataCommandResult
{
    public Guid VoyageId { get; set; }
    public Guid BookingAId { get; set; }
    public Guid BookingBId { get; set; }
    public Guid ClosedVoyageId { get; set; }
    public Guid ClosedVoyageBookingId { get; set; }
    public int VoyageCapacity { get; set; }
    public int BookingAUnits { get; set; }
    public int BookingBUnits { get; set; }
}
