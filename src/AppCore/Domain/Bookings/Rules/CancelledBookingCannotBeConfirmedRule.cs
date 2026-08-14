namespace Admiral.Booking.AppCore.Domain.Bookings.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Bookings.Enums;

public sealed class CancelledBookingCannotBeConfirmedRule(BookingStatus status) : BusinessRule
{
    public override string ErrorCode => "BOOKING_CANCELLED";
    public override string Message => "A cancelled booking cannot be confirmed.";
    public override bool IsBroken() => status == BookingStatus.Cancelled;
}
