namespace Admiral.Booking.AppCore.Shared.OutboxMessages.Commands;

using Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;

public interface IOutboxMessageCommandRepository : ICommandRepository<OutboxMessage, Guid>
{
    Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(int batchSize, DateTimeOffset utcNow, CancellationToken cancellationToken = default);
}
