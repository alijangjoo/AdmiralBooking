namespace Admiral.Booking.Endpoints.Base;

using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.Endpoints.Base.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected async Task<IActionResult> SendRequestAsync<T>(IRequest<QueryResult<T>> request)
    {
        var result = await Mediator.Send(request);
        return result.ToActionResult(this);
    }

    protected async Task<IActionResult> SendCommandAsync<T>(IRequest<CommandResult<T>> request)
    {
        var result = await Mediator.Send(request);
        return result.ToActionResult(this);
    }
}
