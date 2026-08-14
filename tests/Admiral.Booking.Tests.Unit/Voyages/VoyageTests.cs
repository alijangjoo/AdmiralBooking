using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.Domain.Voyages.Enums;
using FluentAssertions;
using Xunit;

namespace AdmiralBooking.Tests.Unit;

public class VoyageTests
{
    [Fact]
    public void Voyage_cannot_reserve_beyond_total_capacity()
    {
        var voyage = OpenVoyage(2);
        voyage.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(2));

        var act = () => voyage.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(1));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "INSUFFICIENT_CAPACITY");
        voyage.ReservedCapacity.Value.Should().Be(2);
    }

    [Fact]
    public void Closed_voyage_cannot_accept_a_reservation()
    {
        var voyage = OpenVoyage(5);
        voyage.CloseForBooking();

        var act = () => voyage.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(1));

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "VOYAGE_NOT_OPEN");
        voyage.Reservations.Should().BeEmpty();
    }

    [Fact]
    public void Departed_or_cancelled_voyage_cannot_accept_reservations()
    {
        var departed = OpenVoyage(5);
        departed.MarkDeparted();
        var cancelled = OpenVoyage(5);
        cancelled.Cancel();

        var departedAct = () => departed.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(1));
        var cancelledAct = () => cancelled.ReserveCapacity(Guid.NewGuid(), ContainerUnits.From(1));

        departedAct.Should().Throw<BusinessRuleValidationException>().Where(ex => ex.ErrorCode == "VOYAGE_NOT_OPEN");
        cancelledAct.Should().Throw<BusinessRuleValidationException>().Where(ex => ex.ErrorCode == "VOYAGE_NOT_OPEN");
    }

    [Fact]
    public void Same_booking_does_not_create_a_second_active_reservation()
    {
        var voyage = OpenVoyage(4);
        var bookingId = Guid.NewGuid();
        var first = voyage.ReserveCapacity(bookingId, ContainerUnits.From(2));
        var second = voyage.ReserveCapacity(bookingId, ContainerUnits.From(2));

        second.Id.Should().Be(first.Id);
        voyage.ReservedCapacity.Value.Should().Be(2);
        voyage.Reservations.Count(x => x.Status == CapacityReservationStatus.Active).Should().Be(1);
    }

    private static Voyage OpenVoyage(int capacity)
        => Voyage.Create(
            Guid.NewGuid(),
            LocationName.From("Hamburg"),
            LocationName.From("Shanghai"),
            DateTimeOffset.UtcNow.AddDays(10),
            ContainerUnits.From(capacity));
}
