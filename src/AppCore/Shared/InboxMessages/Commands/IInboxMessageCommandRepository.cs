namespace Admiral.Booking.AppCore.Shared.InboxMessages.Commands;

using Admiral.Booking.AppCore.Domain.InboxMessages.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;

public interface IInboxMessageCommandRepository : ICommandRepository<InboxMessage, Guid>
{
    Task<bool> ExistsAsync(Guid messageId, string consumerName, CancellationToken cancellationToken = default);
}
