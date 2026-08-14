namespace Admiral.Booking.AppCore.Shared.Bookings.Commands;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.Infra.Persistence.Base.Commands;

public interface IBookingCommandRepository : ICommandRepository<Booking, Guid>
{
    Task<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
