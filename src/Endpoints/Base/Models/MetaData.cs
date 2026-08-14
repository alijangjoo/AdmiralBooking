namespace Admiral.Booking.Endpoints.Base.Models;

public class MetaData
{
    public DateTime Timestamp { get; set; }

    public static MetaData Create() => new() { Timestamp = DateTime.UtcNow };
}
