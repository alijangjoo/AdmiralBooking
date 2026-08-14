namespace Admiral.Booking.AppCore.Domain.Base.Rules;

using Admiral.Booking.AppCore.Domain.Base.Rules.Localizations;

public interface IBusinessRule
{
    bool IsBroken();
    string Message { get; }
    string ErrorCode { get; }
    LocalizationKey? MessageKey => null;
}
