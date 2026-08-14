namespace Admiral.Booking.AppCore.Shared.Base;

public class CommandResult<T> : CommandResult
{
    public T Data { get; set; } = default!;

    public static CommandResult<T> Success(T data)
        => new()
        {
            IsSuccess = true,
            Data = data
        };

    public new static CommandResult<T> Failure(string message, string code)
        => new()
        {
            IsSuccess = false,
            ErrorMessages = [message],
            ErrorCode = code
        };

    public static CommandResult<T> ValidationFailure(List<string> messages, Dictionary<string, string[]> validationErrors)
        => new()
        {
            IsSuccess = false,
            ErrorMessages = messages,
            ValidationErrors = validationErrors,
            ErrorCode = "VALIDATION_ERROR"
        };

    public static CommandResult ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        var errorMessages = validationErrors.SelectMany(x => x.Value).ToList();

        return new CommandResult
        {
            IsSuccess = false,
            ErrorMessages = errorMessages,
            ValidationErrors = validationErrors,
            ErrorCode = "VALIDATION_ERROR"
        };
    }
}

public class CommandResult : IResult
{
    public bool IsSuccess { get; set; }

    public List<string> ErrorMessages { get; set; } = [];

    public Dictionary<string, string[]> ValidationErrors { get; set; } = [];

    public string ErrorCode { get; set; } = string.Empty;

    public string ErrorMessage => ErrorMessages.FirstOrDefault() ?? string.Empty;

    public static CommandResult Success() => new() { IsSuccess = true };

    public static CommandResult Failure(string message, string code)
        => new()
        {
            IsSuccess = false,
            ErrorMessages = [message],
            ErrorCode = code
        };

    public static CommandResult ValidationFailure(List<string> messages, Dictionary<string, string[]> validationErrors)
        => new()
        {
            IsSuccess = false,
            ErrorMessages = messages,
            ValidationErrors = validationErrors,
            ErrorCode = "VALIDATION_ERROR"
        };

    public static CommandResult ValidationFailure(Dictionary<string, string[]> validationErrors)
    {
        var errorMessages = validationErrors.SelectMany(x => x.Value).ToList();

        return new CommandResult
        {
            IsSuccess = false,
            ErrorMessages = errorMessages,
            ValidationErrors = validationErrors,
            ErrorCode = "VALIDATION_ERROR"
        };
    }
}
