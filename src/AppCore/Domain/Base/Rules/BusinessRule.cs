namespace Admiral.Booking.AppCore.Domain.Base.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules.Localizations;

public abstract class BusinessRule : IBusinessRule
{
    public abstract bool IsBroken();
    public abstract string ErrorCode { get; }
    public virtual LocalizationKey? MessageKey => null;
    public virtual string Message => MessageKey?.ToString() ?? string.Empty;

    public void ThrowIfBroken()
    {
        if (IsBroken())
            throw new BusinessRuleValidationException(this);
    }
}
