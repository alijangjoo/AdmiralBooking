namespace Admiral.Booking.Infra.Persistence.MS.Commands.IdempotencyRecords;

using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;
using Admiral.Booking.AppCore.Shared.IdempotencyRecords.Commands;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class IdempotencyRecordCommandRepository(AppCommandDbContext dbContext)
    : CommandRepository<IdempotencyRecord, AppCommandDbContext, Guid>(dbContext), IIdempotencyRecordCommandRepository
{
    public Task<IdempotencyRecord?> FindByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        => _dbContext.IdempotencyRecords.FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
}
