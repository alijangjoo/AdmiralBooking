namespace Admiral.Booking.Infra.Persistence.MS.Commands.Voyages;

using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.Shared.Voyages.Commands;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class VoyageCommandRepository(AppCommandDbContext dbContext)
    : CommandRepository<Voyage, AppCommandDbContext, Guid>(dbContext), IVoyageCommandRepository
{
    public Task<Voyage?> FindGraphAsync(Guid voyageId, CancellationToken cancellationToken = default)
        => _dbContext.Voyages
            .Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == voyageId, cancellationToken);
}
