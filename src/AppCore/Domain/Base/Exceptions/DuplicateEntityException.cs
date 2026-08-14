namespace Admiral.Booking.AppCore.Domain.Base.Exceptions;

public class DuplicateEntityException : DomainException
{
    public Type EntityType { get; }
    public string PropertyName { get; }
    public object PropertyValue { get; }

    public override string ErrorCode => "DUPLICATE_ENTITY";

    public DuplicateEntityException(Type entityType, string propertyName, object propertyValue)
        : base($"{entityType.Name} with {propertyName} '{propertyValue}' already exists.")
    {
        EntityType = entityType;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
        Parameters[nameof(EntityType)] = entityType.Name;
        Parameters[nameof(PropertyName)] = propertyName;
        Parameters[nameof(PropertyValue)] = propertyValue;
    }
}
