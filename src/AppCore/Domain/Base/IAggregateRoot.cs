namespace Admiral.Booking.AppCore.Domain.Base;

using Admiral.Booking.AppCore.Domain.Base.Events;

public interface IAggregateRoot
{
    void ClearEvents();
    IEnumerable<IDomainEvent> GetEvents();
}
