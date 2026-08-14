namespace Admiral.Booking.AppCore.Domain.Voyages.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;

public sealed class DuplicateActiveReservationRule(
    IEnumerable<CapacityReservation> reservations,
    Guid bookingId) : BusinessRule
{
    public override string ErrorCode => "DUPLICATE_RESERVATION";
    public override string Message => "A booking must not reserve the same voyage capacity more than once.";

    public override bool IsBroken()
        => reservations.Any(r => r.BookingId == bookingId && r.Status == CapacityReservationStatus.Active);
}
