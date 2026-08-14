namespace Admiral.Booking.AppCore.Shared.Base;

public class QueryResult<T> : IResult
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; } = default!;
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, string[]> ValidationErrors { get; set; } = [];

    public static QueryResult<T> Success(T data)
        => new()
        {
            IsSuccess = true,
            Data = data
        };

    public static QueryResult<T> Failure(string message, string code)
        => new()
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorCode = code
        };
}
