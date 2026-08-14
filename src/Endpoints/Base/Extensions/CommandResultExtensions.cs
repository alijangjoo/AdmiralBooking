namespace Admiral.Booking.Endpoints.Base.Extensions;

using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.Endpoints.Base.Models;
using Microsoft.AspNetCore.Mvc;

public static class CommandResultExtensions
{
    public static IActionResult ToActionResult<T>(this CommandResult<T> result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<T>.SuccessResponse(result.Data));

        return controller.StatusCode(
            ResultStatusCodeResolver.GetStatusCode(result),
            ApiResponse<T>.FailureResponse(
                result.ErrorMessage ?? "Operation failed.",
                result.ErrorCode ?? "COMMAND_ERROR",
                result.ValidationErrors));
    }

    public static IActionResult ToActionResult(this CommandResult result, ControllerBase controller)
    {
        if (result.IsSuccess)
            return controller.Ok(ApiResponse<bool>.SuccessResponse(true));

        return controller.StatusCode(
            ResultStatusCodeResolver.GetStatusCode(result),
            ApiResponse<object>.FailureResponse(
                result.ErrorMessage ?? "Operation failed.",
                result.ErrorCode ?? "COMMAND_ERROR",
                result.ValidationErrors));
    }
}
