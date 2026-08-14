namespace Admiral.Booking.AppCore.AppServices.OutboxMessages;

using Admiral.Booking.AppCore.Domain.InboxMessages.Entities;
using Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;
using Admiral.Booking.AppCore.Shared.InboxMessages.Commands;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Options;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ConfigurableIntegrationEventPublisher(
    IOptions<OutboxOptions> options,
    PublicationFailureGate failureGate,
    ILogger<ConfigurableIntegrationEventPublisher> logger) : IIntegrationEventPublisher
{
    public Task PublishAsync(BookingConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        if (options.Value.SimulatePublishFailure || failureGate.Fail)
            throw new InvalidOperationException("Simulated integration-event publication failure.");

        logger.LogInformation(
            "Published BookingConfirmed integration event. EventId={EventId} BookingId={BookingId} VoyageId={VoyageId}",
            integrationEvent.EventId,
            integrationEvent.BookingId,
            integrationEvent.VoyageId);

        return Task.CompletedTask;
    }
}

public class IdempotentBookingConfirmedConsumer(
    IInboxMessageCommandRepository inboxMessageCommandRepository,
    ILogger<IdempotentBookingConfirmedConsumer> logger) : IBookingConfirmedConsumer
{
    public static int DownstreamEffectCount;

    public async Task ConsumeAsync(BookingConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        if (await inboxMessageCommandRepository.ExistsAsync(integrationEvent.EventId, nameof(IdempotentBookingConfirmedConsumer), cancellationToken))
        {
            logger.LogInformation(
                "Duplicate BookingConfirmed delivery ignored. EventId={EventId} BookingId={BookingId}",
                integrationEvent.EventId,
                integrationEvent.BookingId);
            return;
        }

        await inboxMessageCommandRepository.InsertAsync(
            InboxMessage.Create(integrationEvent.EventId, nameof(IdempotentBookingConfirmedConsumer)));

        Interlocked.Increment(ref DownstreamEffectCount);
        logger.LogInformation(
            "Consumed BookingConfirmed integration event. EventId={EventId} BookingId={BookingId} DownstreamEffects={DownstreamEffects}",
            integrationEvent.EventId,
            integrationEvent.BookingId,
            DownstreamEffectCount);
    }
}
