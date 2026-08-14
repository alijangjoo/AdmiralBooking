namespace Admiral.Booking.AppCore.Shared.Bookings.Services;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;

public interface IBookingConfirmationService
{
    BookingConfirmationOutcome Confirm(Booking booking, Voyage voyage);
}

public sealed record BookingConfirmationOutcome(Guid ReservationId);
