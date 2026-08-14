namespace Admiral.Booking.AppCore.Domain.Base.Exceptions;

using Admiral.Booking.AppCore.Domain.Base.Exceptions.Enums;

public class EntityNotFoundException : DomainException
{
    public Type EntityType { get; }
    public object EntityId { get; }

    public override string ErrorCode => "NOT_FOUND";
    public override ErrorSeverity Severity => ErrorSeverity.Warning;

    public EntityNotFoundException(Type entityType, object entityId)
        : base($"{entityType.Name} '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
        Parameters[nameof(EntityType)] = entityType.Name;
        Parameters[nameof(EntityId)] = entityId;
    }
}
