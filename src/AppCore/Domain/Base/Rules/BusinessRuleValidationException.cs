namespace Admiral.Booking.AppCore.Domain.Base.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules.Localizations;

public class BusinessRuleValidationException : Exception
{
    public IBusinessRule? BrokenRule { get; }
    public LocalizationKey? LocalizationKey { get; }
    public string ErrorCode { get; }

    public BusinessRuleValidationException(IBusinessRule brokenRule)
        : base(brokenRule.MessageKey?.ToString() ?? brokenRule.Message)
    {
        BrokenRule = brokenRule;
        LocalizationKey = brokenRule.MessageKey;
        ErrorCode = brokenRule.ErrorCode;
    }

    public BusinessRuleValidationException(string message, string errorCode = "BUSINESS_RULE_VIOLATION")
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessRuleValidationException(LocalizationKey key)
        : base(key.ToString())
    {
        LocalizationKey = key;
        ErrorCode = "BUSINESS_RULE_VIOLATION";
    }
}
