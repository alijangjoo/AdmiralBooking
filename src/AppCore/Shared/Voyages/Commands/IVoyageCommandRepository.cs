namespace Admiral.Booking.AppCore.Shared.Voyages.Commands;

using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;

public interface IVoyageCommandRepository : ICommandRepository<Voyage, Guid>
{
    Task<Voyage?> FindGraphAsync(Guid voyageId, CancellationToken cancellationToken = default);
}
