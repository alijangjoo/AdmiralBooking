namespace Admiral.Booking.AppCore.Domain.Voyages.Entities;

using Admiral.Booking.AppCore.Domain.Base;
using Admiral.Booking.AppCore.Domain.Base.Exceptions;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;
using Admiral.Booking.AppCore.Domain.Voyages.Events;
using Admiral.Booking.AppCore.Domain.Voyages.Rules;

public class Voyage : AggregateRoot<Guid>
{
    private readonly List<CapacityReservation> _reservations = [];

    public LocationName Origin { get; private set; } = null!;
    public LocationName Destination { get; private set; } = null!;
    public DateTimeOffset DepartureDate { get; private set; }
    public ContainerUnits TotalCapacity { get; private set; } = ContainerUnits.Zero;
    public ContainerUnits ReservedCapacity { get; private set; } = ContainerUnits.Zero;
    public VoyageStatus Status { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<CapacityReservation> Reservations => _reservations.AsReadOnly();

    public int RemainingCapacity => TotalCapacity.Value - ReservedCapacity.Value;

    private Voyage()
    {
    }

    public static Voyage Create(
        Guid id,
        LocationName origin,
        LocationName destination,
        DateTimeOffset departureDate,
        ContainerUnits totalCapacity)
    {
        return new Voyage
        {
            Id = id,
            Origin = origin,
            Destination = destination,
            DepartureDate = departureDate,
            TotalCapacity = totalCapacity,
            ReservedCapacity = ContainerUnits.Zero,
            Status = VoyageStatus.OpenForBooking,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public CapacityReservation ReserveCapacity(Guid bookingId, ContainerUnits requestedUnits)
    {
        CheckRule(new VoyageMustBeOpenForBookingRule(Status));

        var existing = _reservations.FirstOrDefault(r =>
            r.BookingId == bookingId && r.Status == CapacityReservationStatus.Active);

        if (existing is not null)
            return existing;

        if (!ReservedCapacity.CanAdd(requestedUnits, TotalCapacity))
        {
            AddEvent(new CapacityReservationRejected(
                Id,
                bookingId,
                requestedUnits.Value,
                RemainingCapacity,
                "Insufficient voyage capacity."));

            CheckRule(new ReservedCapacityCannotExceedTotalRule(TotalCapacity, ReservedCapacity, requestedUnits));
        }

        var reservation = CapacityReservation.Create(Guid.NewGuid(), Id, bookingId, requestedUnits);
        _reservations.Add(reservation);
        ReservedCapacity = ReservedCapacity.Add(requestedUnits);
        Version++;

        AddEvent(new CapacityReserved(
            Id,
            bookingId,
            reservation.Id,
            requestedUnits.Value,
            ReservedCapacity.Value));

        return reservation;
    }

    public void ReleaseCapacity(Guid bookingId)
    {
        var reservation = _reservations.FirstOrDefault(r =>
            r.BookingId == bookingId && r.Status == CapacityReservationStatus.Active);

        if (reservation is null)
            return;

        reservation.Release();
        ReservedCapacity = ReservedCapacity.Subtract(reservation.ReservedUnits);
        Version++;
        AddEvent(new CapacityReleased(Id, bookingId, reservation.Id, reservation.ReservedUnits.Value));
    }

    public void ConsumeReservation(Guid bookingId)
    {
        var reservation = _reservations.FirstOrDefault(r =>
            r.BookingId == bookingId && r.Status == CapacityReservationStatus.Active);

        reservation?.Consume();
    }

    public void CloseForBooking()
    {
        EnsureMutableSchedule();
        Status = VoyageStatus.ClosedForBooking;
        Version++;
    }

    public void MarkDeparted()
    {
        EnsureMutableSchedule();
        Status = VoyageStatus.Departed;
        Version++;
    }

    public void Cancel()
    {
        EnsureMutableSchedule();
        Status = VoyageStatus.Cancelled;
        Version++;
    }

    private void EnsureMutableSchedule()
    {
        if (Status is VoyageStatus.Departed or VoyageStatus.Cancelled)
            throw new InvalidEntityStateException(typeof(Voyage), Status.ToString(), "OpenForBooking or ClosedForBooking");
    }
}
