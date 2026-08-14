namespace Admiral.Booking.AppCore.Domain.Bookings.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;

public sealed class BookingMustHaveCapacityReservationRule(Guid? reservationId) : BusinessRule
{
    public override string ErrorCode => "CAPACITY_RESERVATION_REQUIRED";
    public override string Message => "A booking cannot become confirmed without a valid capacity reservation.";
    public override bool IsBroken() => reservationId is null || reservationId == Guid.Empty;
}
