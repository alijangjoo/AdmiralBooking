namespace Admiral.Booking.AppCore.AppServices.Bookings.Commands.Confirm;

using Admiral.Booking.AppCore.Shared.Bookings.Commands.Confirm;
using FluentValidation;

public class ConfirmBookingCommandValidator : AbstractValidator<ConfirmBookingCommand>
{
    public ConfirmBookingCommandValidator()
    {
        RuleFor(x => x.BookingId).NotEmpty();
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency-Key is required.")
            .MaximumLength(128);
    }
}
