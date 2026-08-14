namespace Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;

public sealed class BookingConfirmedIntegrationEvent
{
    public const string EventType = "booking.confirmed.v1";

    public Guid EventId { get; init; }
    public string EventTypeName { get; init; } = EventType;
    public int Version { get; init; } = 1;
    public Guid BookingId { get; init; }
    public Guid CustomerId { get; init; }
    public Guid VoyageId { get; init; }
    public Guid ReservationId { get; init; }
    public int ReservedUnits { get; init; }
    public DateTimeOffset ConfirmedAt { get; init; }
    public string? CorrelationId { get; init; }
}
