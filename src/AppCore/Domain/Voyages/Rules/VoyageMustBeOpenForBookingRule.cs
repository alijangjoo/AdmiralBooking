namespace Admiral.Booking.AppCore.Domain.Voyages.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;

public sealed class VoyageMustBeOpenForBookingRule(VoyageStatus status) : BusinessRule
{
    public override string ErrorCode => "VOYAGE_NOT_OPEN";
    public override string Message => "The voyage is not open for booking and cannot accept capacity reservations.";
    public override bool IsBroken() => status != VoyageStatus.OpenForBooking;
}