namespace Admiral.Booking.AppCore.Domain.Common.ValueObjects;

using Admiral.Booking.AppCore.Domain.Base.ValueObjects;
using Admiral.Booking.AppCore.Domain.Common.Rules;

public sealed class LocationName : ValueObject
{
    public string Value { get; }

    private LocationName(string value) => Value = value;

    public static LocationName From(string value)
    {
        CheckRule(new LocationNameMustBeProvidedRule(value));
        return new LocationName(value.Trim());
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}