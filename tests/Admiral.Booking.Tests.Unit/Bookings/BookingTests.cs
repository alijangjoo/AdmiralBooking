using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.DomainServices.Bookings;
using FluentAssertions;
using Xunit;
using BookingAggregate = Admiral.Booking.AppCore.Domain.Bookings.Entities.Booking;

namespace AdmiralBooking.Tests.Unit;

public class BookingTests
{
    [Fact]
    public void Cancelled_booking_cannot_be_confirmed()
    {
        var booking = BookingAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ContainerUnits.From(2));
        booking.Cancel();

        var act = () => booking.StartConfirmation();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "BOOKING_CANCELLED");
    }

    [Fact]
    public void Confirmed_booking_cannot_be_confirmed_again()
    {
        var (booking, voyage) = ConfirmablePair(2);
        new BookingConfirmationService().Confirm(booking, voyage);

        var act = () => booking.StartConfirmation();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "BOOKING_ALREADY_CONFIRMED");
    }

    [Fact]
    public void Booking_cannot_confirm_without_a_valid_capacity_reservation()
    {
        var booking = BookingAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), ContainerUnits.From(1));
        booking.StartConfirmation();

        var act = () => booking.Confirm();

        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "CAPACITY_RESERVATION_REQUIRED");
    }

    [Fact]
    public void Requested_capacity_must_be_greater_than_zero()
    {
        var act = () => ContainerUnits.From(0);
        act.Should().Throw<BusinessRuleValidationException>()
            .Where(ex => ex.ErrorCode == "REQUESTED_UNITS_INVALID");
    }

    private static (BookingAggregate booking, Voyage voyage) ConfirmablePair(int units)
    {
        var voyage = Voyage.Create(
            Guid.NewGuid(),
            LocationName.From("Rotterdam"),
            LocationName.From("Singapore"),
            DateTimeOffset.UtcNow.AddDays(7),
            ContainerUnits.From(units));
        var booking = BookingAggregate.Create(Guid.NewGuid(), Guid.NewGuid(), voyage.Id, ContainerUnits.From(units));
        return (booking, voyage);
    }
}
