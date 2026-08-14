namespace Admiral.Booking.AppCore.Domain.Common.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;

public sealed class RequestedUnitsMustBePositiveRule(int units) : BusinessRule
{
    public override string ErrorCode => "REQUESTED_UNITS_INVALID";
    public override string Message => "Requested capacity must be greater than zero.";
    public override bool IsBroken() => units <= 0;
}