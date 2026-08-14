namespace Admiral.Booking.AppCore.Domain.Bookings.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;

public sealed class BookingMustReferenceVoyageRule(Guid voyageId) : BusinessRule
{
    public override string ErrorCode => "BOOKING_VOYAGE_REQUIRED";
    public override string Message => "A booking must reference a voyage.";
    public override bool IsBroken() => voyageId == Guid.Empty;
}