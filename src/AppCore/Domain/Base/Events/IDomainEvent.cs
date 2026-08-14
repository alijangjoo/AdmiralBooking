namespace Admiral.Booking.AppCore.Domain.Base.Events;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
