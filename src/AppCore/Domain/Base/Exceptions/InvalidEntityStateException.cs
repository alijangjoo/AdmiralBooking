namespace Admiral.Booking.AppCore.Domain.Base.Exceptions;

using Admiral.Booking.AppCore.Domain.Base.Exceptions.Enums;

public class InvalidEntityStateException : DomainException
{
    public Type EntityType { get; }
    public string CurrentState { get; }
    public string RequiredState { get; }

    public override string ErrorCode => "INVALID_STATE_TRANSITION";
    public override ErrorSeverity Severity => ErrorSeverity.Error;

    public InvalidEntityStateException(Type entityType, string currentState, string requiredState)
        : base($"{entityType.Name} is in '{currentState}' and cannot move to '{requiredState}'.")
    {
        EntityType = entityType;
        CurrentState = currentState;
        RequiredState = requiredState;
        Parameters[nameof(EntityType)] = entityType.Name;
        Parameters[nameof(CurrentState)] = currentState;
        Parameters[nameof(RequiredState)] = requiredState;
    }
}
