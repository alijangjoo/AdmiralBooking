namespace Admiral.Booking.Infra.Persistence.MS.Queries.Voyages.Entities;

public class Voyage
{
    public Guid Id { get; set; }
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int ReservedCapacity { get; set; }
    public int Status { get; set; }
    public int Version { get; set; }
}
