namespace Admiral.Booking.AppCore.Domain.Voyages.Entities;

using Admiral.Booking.AppCore.Domain.Base;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;

public class CapacityReservation : Entity<Guid>
{
    public Guid VoyageId { get; private set; }
    public Guid BookingId { get; private set; }
    public ContainerUnits ReservedUnits { get; private set; } = ContainerUnits.Zero;
    public CapacityReservationStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private CapacityReservation() { }

    internal static CapacityReservation Create(Guid id, Guid voyageId, Guid bookingId, ContainerUnits reservedUnits)
        => new()
        {
            Id = id,
            VoyageId = voyageId,
            BookingId = bookingId,
            ReservedUnits = reservedUnits,
            Status = CapacityReservationStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

    internal void Consume()
    {
        if (Status != CapacityReservationStatus.Active)
            return;

        Status = CapacityReservationStatus.Consumed;
    }

    internal void Release()
    {
        if (Status != CapacityReservationStatus.Active)
            return;

        Status = CapacityReservationStatus.Released;
    }
}