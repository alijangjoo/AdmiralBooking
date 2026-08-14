namespace Admiral.Booking.AppCore.Shared.OutboxMessages.Services;

using Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(BookingConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

public interface IBookingConfirmedConsumer
{
    Task ConsumeAsync(BookingConfirmedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
