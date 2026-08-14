namespace Admiral.Booking.AppCore.AppServices.OutboxMessages;

using Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Commands;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Options;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class OutboxProcessor(
    IOutboxMessageCommandRepository outboxMessageCommandRepository,
    IIntegrationEventPublisher publisher,
    IBookingConfirmedConsumer consumer,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<int> ProcessPendingAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;
        var pending = await outboxMessageCommandRepository.GetPendingAsync(
            settings.BatchSize,
            DateTimeOffset.UtcNow,
            cancellationToken);

        var published = 0;
        foreach (var message in pending)
        {
            try
            {
                if (!string.Equals(message.EventType, BookingConfirmedIntegrationEvent.EventType, StringComparison.Ordinal))
                {
                    logger.LogWarning("Skipping unsupported outbox event type {EventType}", message.EventType);
                    continue;
                }

                var integrationEvent = JsonSerializer.Deserialize<BookingConfirmedIntegrationEvent>(message.Payload, JsonOptions)
                    ?? throw new InvalidOperationException("Outbox payload could not be deserialized.");

                logger.LogInformation(
                    "Publishing outbox message. OutboxId={OutboxId} BookingId={BookingId} Attempt={Attempt}",
                    message.Id,
                    integrationEvent.BookingId,
                    message.AttemptCount + 1);

                await publisher.PublishAsync(integrationEvent, cancellationToken);
                await consumer.ConsumeAsync(integrationEvent, cancellationToken);
                message.MarkPublished();
                published++;
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Outbox publication failed. OutboxId={OutboxId} Attempt={Attempt}",
                    message.Id,
                    message.AttemptCount + 1);

                message.MarkFailed(ex.Message, TimeSpan.FromSeconds(settings.RetryDelaySeconds), settings.MaxAttempts);
            }
        }

        if (pending.Count > 0)
            await outboxMessageCommandRepository.CommitAsync();

        return published;
    }
}
