namespace Admiral.Booking.AppCore.Domain.Voyages.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;

public sealed class ReservedCapacityCannotExceedTotalRule(
    ContainerUnits totalCapacity,
    ContainerUnits reservedCapacity,
    ContainerUnits requestedUnits) : BusinessRule
{
    public override string ErrorCode => "INSUFFICIENT_CAPACITY";
    public override string Message =>
        $"Insufficient voyage capacity. Requested {requestedUnits.Value} units, remaining {Math.Max(0, totalCapacity.Value - reservedCapacity.Value)} units.";

    public override bool IsBroken()
        => reservedCapacity.Value + requestedUnits.Value > totalCapacity.Value;
}
