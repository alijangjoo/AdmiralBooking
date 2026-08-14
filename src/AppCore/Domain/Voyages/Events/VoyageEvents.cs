namespace Admiral.Booking.AppCore.Domain.Voyages.Events;

using Admiral.Booking.AppCore.Domain.Base.Events;

public sealed record CapacityReserved(
    Guid VoyageId,
    Guid BookingId,
    Guid ReservationId,
    int ReservedUnits,
    int TotalReserved) : DomainEvent;

public sealed record CapacityReservationRejected(
    Guid VoyageId,
    Guid BookingId,
    int RequestedUnits,
    int RemainingUnits,
    string Reason) : DomainEvent;

public sealed record CapacityReleased(Guid VoyageId, Guid BookingId, Guid ReservationId, int ReleasedUnits) : DomainEvent;
