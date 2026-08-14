namespace Admiral.Booking.Endpoints.Base.Extensions;

using Admiral.Booking.AppCore.Shared.Base;
using Microsoft.AspNetCore.Http;

public static class ResultStatusCodeResolver
{
    public static int GetStatusCode(IResult result)
    {
        if (result.ValidationErrors?.Count > 0)
            return StatusCodes.Status400BadRequest;

        var code = result.ErrorCode?.ToUpperInvariant() ?? string.Empty;

        if (code is "VALIDATION_ERROR" or "BAD_REQUEST" or "REQUESTED_UNITS_INVALID" or "IDEMPOTENCY_KEY_REQUIRED")
            return StatusCodes.Status400BadRequest;

        if (code is "NOT_FOUND" || code.EndsWith("_NOT_FOUND", StringComparison.Ordinal))
            return StatusCodes.Status404NotFound;

        if (code is "CONFLICT"
            or "CONCURRENCY_CONFLICT"
            or "IDEMPOTENCY_KEY_CONFLICT"
            or "IDEMPOTENCY_IN_PROGRESS"
            or "INSUFFICIENT_CAPACITY"
            or "VOYAGE_NOT_OPEN"
            or "BOOKING_ALREADY_CONFIRMED"
            or "BOOKING_CANCELLED"
            or "DUPLICATE_RESERVATION"
            or "CAPACITY_RESERVATION_REQUIRED"
            or "INVALID_STATE_TRANSITION")
            return StatusCodes.Status409Conflict;

        if (!string.IsNullOrWhiteSpace(code) && code is not ("COMMAND_ERROR" or "FAILED-CODE"))
            return StatusCodes.Status400BadRequest;

        return StatusCodes.Status500InternalServerError;
    }
}
