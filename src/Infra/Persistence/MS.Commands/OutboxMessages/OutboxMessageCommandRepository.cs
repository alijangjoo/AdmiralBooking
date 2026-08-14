namespace Admiral.Booking.Infra.Persistence.MS.Commands.OutboxMessages;

using Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;
using Admiral.Booking.AppCore.Domain.OutboxMessages.Enums;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Commands;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class OutboxMessageCommandRepository(AppCommandDbContext dbContext)
    : CommandRepository<OutboxMessage, AppCommandDbContext, Guid>(dbContext), IOutboxMessageCommandRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> GetPendingAsync(
        int batchSize,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken = default)
        => await _dbContext.OutboxMessages
            .Where(x => x.Status == OutboxStatus.Pending && (x.NextAttemptAt == null || x.NextAttemptAt <= utcNow))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
}
