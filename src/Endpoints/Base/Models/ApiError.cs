namespace Admiral.Booking.Endpoints.Base.Models;

public class ApiError
{
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Dictionary<string, string[]> ValidationErrors { get; set; } = [];
}
