namespace Admiral.Booking.Endpoints.Api.Bookings;

using Admiral.Booking.AppCore.Shared.Bookings.Commands.Confirm;
using Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;
using Admiral.Booking.Endpoints.Base;
using Admiral.Booking.Endpoints.Base.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/bookings")]
[ApiController]
public class BookingsController(IMediator mediator) : BaseApiController(mediator)
{
    [HttpPost("{bookingId:guid}/confirm")]
    public async Task<IActionResult> Confirm(
        [FromRoute] Guid bookingId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        [FromHeader(Name = CorrelationIdMiddleware.HeaderName)] string? correlationId,
        [FromBody] ConfirmBookingRequest? body)
    {
        return await SendCommandAsync(new ConfirmBookingCommand
        {
            BookingId = bookingId,
            IdempotencyKey = idempotencyKey ?? string.Empty,
            ExpectedVersion = body?.ExpectedVersion,
            CorrelationId = correlationId ?? HttpContext.Items[CorrelationIdMiddleware.HeaderName]?.ToString()
        });
    }

    [HttpGet("{bookingId:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid bookingId)
        => await SendRequestAsync(new GetBookingByIdQuery { BookingId = bookingId });
}

public class ConfirmBookingRequest
{
    public int? ExpectedVersion { get; set; }
}
