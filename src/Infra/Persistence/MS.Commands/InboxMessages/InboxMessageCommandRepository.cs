namespace Admiral.Booking.Infra.Persistence.MS.Commands.InboxMessages;

using Admiral.Booking.AppCore.Domain.InboxMessages.Entities;
using Admiral.Booking.AppCore.Shared.InboxMessages.Commands;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class InboxMessageCommandRepository(AppCommandDbContext dbContext)
    : CommandRepository<InboxMessage, AppCommandDbContext, Guid>(dbContext), IInboxMessageCommandRepository
{
    public Task<bool> ExistsAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default)
        => _dbContext.InboxMessages.AnyAsync(
            x => x.Id == messageId && x.ConsumerName == consumerName,
            cancellationToken);
}
