namespace Admiral.Booking.AppCore.Domain.Base.Exceptions;

using Admiral.Booking.AppCore.Domain.Base.Exceptions.Enums;

public abstract class DomainException : Exception
{
    public virtual string ErrorCode { get; } = "DOMAIN_ERROR";
    public virtual ErrorSeverity Severity { get; } = ErrorSeverity.Error;
    public virtual Dictionary<string, object> Parameters { get; } = new();

    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected DomainException(string message, string errorCode, ErrorSeverity severity = ErrorSeverity.Error)
        : base(message)
    {
        ErrorCode = errorCode;
        Severity = severity;
    }
}
