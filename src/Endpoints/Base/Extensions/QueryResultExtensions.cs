namespace Admiral.Booking.Endpoints.Base.Extensions;

using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.Endpoints.Base.Models;
using Microsoft.AspNetCore.Mvc;

public static class QueryResultExtensions
{
    public static IActionResult ToActionResult<T>(this QueryResult<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<T>.SuccessResponse(result.Data));

        return controller.StatusCode(
            ResultStatusCodeResolver.GetStatusCode(result),
            ApiResponse<T>.FailureResponse(
                result.ErrorMessage,
                result.ErrorCode,
                result.ValidationErrors));
    }
}
