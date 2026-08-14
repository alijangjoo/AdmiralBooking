namespace Admiral.Booking.Endpoints.Api.Dev;

using Admiral.Booking.AppCore.AppServices.OutboxMessages;
using Admiral.Booking.AppCore.Shared.Bookings.Commands.Seed;
using Admiral.Booking.Endpoints.Base;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/dev")]
[ApiController]
public class DevController(IMediator mediator, OutboxProcessor outboxProcessor) : BaseApiController(mediator)
{
    [HttpPost("seed")]
    public async Task<IActionResult> Seed([FromBody] SeedSampleDataCommand? command)
        => await SendCommandAsync(command ?? new SeedSampleDataCommand());

    [HttpPost("outbox/process")]
    public async Task<IActionResult> ProcessOutbox(CancellationToken cancellationToken)
    {
        var published = await outboxProcessor.ProcessPendingAsync(cancellationToken);
        return Ok(new { published });
    }
}
