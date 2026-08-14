namespace Admiral.Booking.Endpoints.Base.Validations;

using Admiral.Booking.AppCore.Shared.Base;
using FluentValidation;
using MediatR;
using System.Reflection;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count == 0)
            return await next();

        var validationErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        return CreateValidationErrorResponse<TResponse>(validationErrors);
    }

    private static TResponse CreateValidationErrorResponse<TResponse>(Dictionary<string, string[]> validationErrors)
    {
        var responseType = typeof(TResponse);

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(CommandResult<>))
        {
            var genericArg = responseType.GetGenericArguments()[0];
            var commandResultType = typeof(CommandResult<>).MakeGenericType(genericArg);
            var method = commandResultType.GetMethod(
                "ValidationFailure",
                BindingFlags.Public | BindingFlags.Static,
                [typeof(Dictionary<string, string[]>)]);

            if (method is not null)
                return (TResponse)method.Invoke(null, [validationErrors])!;
        }

        if (responseType == typeof(CommandResult))
            return (TResponse)(object)CommandResult.ValidationFailure(validationErrors);

        throw new InvalidOperationException($"Type {responseType.Name} is not supported for validation errors");
    }
}
