namespace Admiral.Booking.AppCore.Domain.Base.Events;

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
