namespace Admiral.Booking.Endpoints.Base.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<ApiError> Errors { get; set; } = [];
    public MetaData Meta { get; set; } = new();

    public static ApiResponse<T> SuccessResponse(T data)
        => new()
        {
            Success = true,
            Data = data,
            Meta = MetaData.Create()
        };

    public static ApiResponse<T> FailureResponse(
        string message,
        string code,
        Dictionary<string, string[]>? validationErrors = null)
        => new()
        {
            Success = false,
            Errors =
            [
                new ApiError
                {
                    Message = message,
                    Code = code,
                    ValidationErrors = validationErrors ?? []
                }
            ],
            Meta = MetaData.Create()
        };
}
