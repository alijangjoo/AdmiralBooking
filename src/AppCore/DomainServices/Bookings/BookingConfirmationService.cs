namespace Admiral.Booking.AppCore.DomainServices.Bookings;

using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.Shared.Bookings.Services;

public class BookingConfirmationService : IBookingConfirmationService
{
    public BookingConfirmationOutcome Confirm(Booking booking, Voyage voyage)
    {
        if (booking.VoyageId != voyage.Id)
            throw new BusinessRuleValidationException("The booking does not reference the supplied voyage.", "BOOKING_VOYAGE_MISMATCH");

        booking.StartConfirmation();

        try
        {
            var reservation = voyage.ReserveCapacity(booking.Id, booking.RequestedUnits);
            booking.AttachCapacityReservation(reservation.Id);
            booking.Confirm();
            voyage.ConsumeReservation(booking.Id);
            return new BookingConfirmationOutcome(reservation.Id);
        }
        catch (Exception)
        {
            booking.AbortConfirmation("Capacity reservation failed.");
            throw;
        }
    }
}
