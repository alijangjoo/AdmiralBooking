namespace Admiral.Booking.AppCore.Domain.Common.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;

public sealed class LocationNameMustBeProvidedRule(string? value) : BusinessRule
{
    public override string ErrorCode => "LOCATION_INVALID";
    public override string Message => "Origin and destination must be provided.";
    public override bool IsBroken() => string.IsNullOrWhiteSpace(value);
}
