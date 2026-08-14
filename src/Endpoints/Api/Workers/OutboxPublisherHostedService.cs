namespace Admiral.Booking.Endpoints.Api.Workers;

using Admiral.Booking.AppCore.AppServices.OutboxMessages;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Options;
using Microsoft.Extensions.Options;

public class OutboxPublisherHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PublisherIntervalSeconds));
        logger.LogInformation("Outbox publisher started. IntervalSeconds={IntervalSeconds}", interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();
                var published = await processor.ProcessPendingAsync(stoppingToken);
                if (published > 0)
                    logger.LogInformation("Outbox publisher published {PublishedCount} message(s).", published);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox publisher loop failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
