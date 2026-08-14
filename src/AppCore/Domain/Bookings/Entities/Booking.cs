namespace Admiral.Booking.AppCore.Domain.Bookings.Entities;

using Admiral.Booking.AppCore.Domain.Base;
using Admiral.Booking.AppCore.Domain.Base.Exceptions;
using Admiral.Booking.AppCore.Domain.Bookings.Enums;
using Admiral.Booking.AppCore.Domain.Bookings.Events;
using Admiral.Booking.AppCore.Domain.Bookings.Rules;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;

public class Booking : AggregateRoot<Guid>
{
    public Guid CustomerId { get; private set; }
    public Guid VoyageId { get; private set; }
    public ContainerUnits RequestedUnits { get; private set; } = ContainerUnits.Zero;
    public BookingStatus Status { get; private set; }
    public Guid? CapacityReservationId { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Booking()
    {
    }

    public static Booking Create(Guid id, Guid customerId, Guid voyageId, ContainerUnits requestedUnits)
    {
        CheckRule(new BookingMustReferenceVoyageRule(voyageId));

        return new Booking
        {
            Id = id,
            CustomerId = customerId,
            VoyageId = voyageId,
            RequestedUnits = requestedUnits,
            Status = BookingStatus.Draft,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void StartConfirmation()
    {
        CheckRule(new CancelledBookingCannotBeConfirmedRule(Status));
        CheckRule(new ConfirmedBookingCannotBeConfirmedAgainRule(Status));

        if (Status is BookingStatus.PendingCapacity or BookingStatus.CapacityReserved)
            return;

        if (Status is BookingStatus.Rejected)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), nameof(BookingStatus.Draft));

        if (Status != BookingStatus.Draft)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), nameof(BookingStatus.Draft));

        Status = BookingStatus.PendingCapacity;
        Version++;
        AddEvent(new BookingConfirmationStarted(Id, VoyageId));
    }

    public void AttachCapacityReservation(Guid reservationId)
    {
        CheckRule(new CancelledBookingCannotBeConfirmedRule(Status));
        CheckRule(new ConfirmedBookingCannotBeConfirmedAgainRule(Status));

        if (reservationId == Guid.Empty)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), "CapacityReserved");

        if (CapacityReservationId.HasValue && CapacityReservationId != reservationId)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), "single active reservation");

        if (Status is not (BookingStatus.PendingCapacity or BookingStatus.CapacityReserved))
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), nameof(BookingStatus.PendingCapacity));

        CapacityReservationId = reservationId;
        Status = BookingStatus.CapacityReserved;
        Version++;
        AddEvent(new BookingCapacityReserved(Id, reservationId, RequestedUnits.Value));
    }

    public void Confirm()
    {
        CheckRule(new CancelledBookingCannotBeConfirmedRule(Status));
        CheckRule(new ConfirmedBookingCannotBeConfirmedAgainRule(Status));
        CheckRule(new BookingMustHaveCapacityReservationRule(CapacityReservationId));

        if (Status != BookingStatus.CapacityReserved)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), nameof(BookingStatus.CapacityReserved));

        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
        Version++;
        AddEvent(new BookingConfirmed(
            Id,
            CustomerId,
            VoyageId,
            RequestedUnits.Value,
            CapacityReservationId!.Value,
            ConfirmedAt.Value));
    }

    public void AbortConfirmation(string reason)
    {
        if (Status != BookingStatus.PendingCapacity)
            return;

        Status = BookingStatus.Draft;
        Version++;
        AddEvent(new BookingConfirmationAborted(Id, reason));
    }

    public void Cancel()
    {
        if (Status is BookingStatus.Confirmed or BookingStatus.Cancelled)
            throw new InvalidEntityStateException(typeof(Booking), Status.ToString(), nameof(BookingStatus.Draft));

        Status = BookingStatus.Cancelled;
        Version++;
        AddEvent(new BookingCancelled(Id));
    }
}
