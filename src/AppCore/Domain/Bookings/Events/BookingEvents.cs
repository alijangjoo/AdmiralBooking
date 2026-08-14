namespace Admiral.Booking.AppCore.Domain.Bookings.Events;

using Admiral.Booking.AppCore.Domain.Base.Events;

public sealed record BookingConfirmationStarted(Guid BookingId, Guid VoyageId) : DomainEvent;

public sealed record BookingCapacityReserved(Guid BookingId, Guid ReservationId, int ReservedUnits) : DomainEvent;

public sealed record BookingConfirmed(
    Guid BookingId,
    Guid CustomerId,
    Guid VoyageId,
    int ReservedUnits,
    Guid ReservationId,
    DateTimeOffset ConfirmedAt) : DomainEvent;

public sealed record BookingCancelled(Guid BookingId) : DomainEvent;

public sealed record BookingConfirmationAborted(Guid BookingId, string Reason) : DomainEvent;
