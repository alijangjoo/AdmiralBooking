namespace Admiral.Booking.AppCore.Domain.Base;

using Admiral.Booking.AppCore.Domain.Base.Events;
using System.Reflection;

public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot
    where TId : struct, IComparable, IComparable<TId>, IEquatable<TId>
{
    private readonly List<IDomainEvent> _events;

    protected AggregateRoot() => _events = [];

    public AggregateRoot(IEnumerable<IDomainEvent> events)
    {
        _events = [];
        if (events is null || !events.Any())
            return;

        foreach (var @event in events)
            Mutate(@event);
    }

    protected void Apply(IDomainEvent @event)
    {
        Mutate(@event);
        AddEvent(@event);
    }

    private void Mutate(IDomainEvent @event)
    {
        var onMethod = GetType().GetMethod("On", BindingFlags.Instance | BindingFlags.NonPublic, [@event.GetType()]);
        onMethod?.Invoke(this, [@event]);
    }

    protected void AddEvent(IDomainEvent @event) => _events.Add(@event);

    public IEnumerable<IDomainEvent> GetEvents() => _events.AsEnumerable();

    public void ClearEvents() => _events.Clear();
}

public abstract class AggregateRoot : AggregateRoot<int>
{
}
