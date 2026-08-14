namespace Admiral.Booking.AppCore.Shared.Base;

public interface IResult
{
    bool IsSuccess { get; }
    Dictionary<string, string[]> ValidationErrors { get; }
    string ErrorCode { get; }
    string ErrorMessage { get; }
}
