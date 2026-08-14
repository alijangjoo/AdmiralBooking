namespace Admiral.Booking.AppCore.Shared.Bookings.Commands.Confirm;

using Admiral.Booking.AppCore.Shared.Base;
using MediatR;

public class ConfirmBookingCommand : IRequest<CommandResult<ConfirmBookingCommandResult>>
{
    public Guid BookingId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public int? ExpectedVersion { get; set; }
    public string? CorrelationId { get; set; }
}
