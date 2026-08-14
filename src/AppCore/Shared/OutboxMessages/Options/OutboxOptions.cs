namespace Admiral.Booking.AppCore.Shared.OutboxMessages.Options;

public class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool SimulatePublishFailure { get; set; }
    public int PublisherIntervalSeconds { get; set; } = 2;
    public int BatchSize { get; set; } = 20;
    public int MaxAttempts { get; set; } = 8;
    public int RetryDelaySeconds { get; set; } = 5;
}
