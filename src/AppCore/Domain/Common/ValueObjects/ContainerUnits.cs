namespace Admiral.Booking.AppCore.Domain.Common.ValueObjects;

using Admiral.Booking.AppCore.Domain.Base.ValueObjects;
using Admiral.Booking.AppCore.Domain.Common.Rules;

public sealed class ContainerUnits : ValueObject
{
    public int Value { get; }

    private ContainerUnits(int value) => Value = value;

    public static ContainerUnits Zero { get; } = new(0);

    public static ContainerUnits From(int value)
    {
        CheckRule(new RequestedUnitsMustBePositiveRule(value));
        return new ContainerUnits(value);
    }

    public ContainerUnits Add(ContainerUnits other)
    {
        var total = Value + other.Value;
        return total == 0 ? Zero : From(total);
    }

    public ContainerUnits Subtract(ContainerUnits other)
    {
        var remaining = Value - other.Value;
        if (remaining < 0)
            throw new InvalidOperationException("Reserved capacity cannot become negative.");

        return remaining == 0 ? Zero : new ContainerUnits(remaining);
    }

    public bool CanAdd(ContainerUnits other, ContainerUnits maximum) => Value + other.Value <= maximum.Value;

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
