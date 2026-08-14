namespace Admiral.Booking.AppCore.Shared.IdempotencyRecords.Commands;

using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;

public interface IIdempotencyRecordCommandRepository : ICommandRepository<IdempotencyRecord, Guid>
{
    Task<IdempotencyRecord?> FindByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
}
