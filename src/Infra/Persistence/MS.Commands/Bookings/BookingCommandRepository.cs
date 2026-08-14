namespace Admiral.Booking.Infra.Persistence.MS.Commands.Bookings;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Shared.Bookings.Commands;
using Admiral.Booking.Infra.Persistence.Base.Commands;
using Microsoft.EntityFrameworkCore;

public class BookingCommandRepository(AppCommandDbContext dbContext)
    : CommandRepository<Booking, AppCommandDbContext, Guid>(dbContext), IBookingCommandRepository
{
    public Task<Booking?> FindAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => _dbContext.Bookings.FirstOrDefaultAsync(x => x.Id == bookingId, cancellationToken);
}
