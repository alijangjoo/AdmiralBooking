namespace Admiral.Booking.Endpoints.Base.Middlewares;

using Admiral.Booking.AppCore.Domain.Base.Exceptions;
using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.Endpoints.Base.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

public class BusinessRuleExceptionMiddleware(RequestDelegate next, ILogger<BusinessRuleExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        int statusCode;
        ApiResponse<object> apiResponse;

        switch (exception)
        {
            case BusinessRuleValidationException brve:
                statusCode = StatusCodes.Status409Conflict;
                apiResponse = ApiResponse<object>.FailureResponse(brve.Message, brve.ErrorCode);
                break;
            case EntityNotFoundException notFound:
                statusCode = StatusCodes.Status404NotFound;
                apiResponse = ApiResponse<object>.FailureResponse(notFound.Message, notFound.ErrorCode);
                break;
            case DomainException domainEx:
                statusCode = StatusCodes.Status400BadRequest;
                apiResponse = ApiResponse<object>.FailureResponse(domainEx.Message, domainEx.ErrorCode);
                break;
            default:
                logger.LogError(exception, "Unhandled exception");
                statusCode = (int)HttpStatusCode.InternalServerError;
                apiResponse = ApiResponse<object>.FailureResponse("An unexpected error occurred.", "INTERNAL_SERVER_ERROR");
                break;
        }

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse, JsonOptions));
    }
}
