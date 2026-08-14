namespace Admiral.Booking.AppCore.Domain.Bookings.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Bookings.Enums;

public sealed class ConfirmedBookingCannotBeConfirmedAgainRule(BookingStatus status) : BusinessRule
{
    public override string ErrorCode => "BOOKING_ALREADY_CONFIRMED";
    public override string Message => "A confirmed booking cannot be confirmed again.";
    public override bool IsBroken() => status == BookingStatus.Confirmed;
}