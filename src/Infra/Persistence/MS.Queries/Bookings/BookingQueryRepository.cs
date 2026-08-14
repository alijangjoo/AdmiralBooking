namespace Admiral.Booking.Infra.Persistence.MS.Queries.Bookings;

using Admiral.Booking.AppCore.Domain.Bookings.Enums;
using Admiral.Booking.AppCore.Shared.Bookings.Queries;
using Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;
using Admiral.Booking.Infra.Persistence.Base.Queries;
using Microsoft.EntityFrameworkCore;

public class BookingQueryRepository(AppQueryDbContext dbContext)
    : QueryRepository<AppQueryDbContext>(dbContext), IBookingQueryRepository
{
    public async Task<GetBookingByIdQueryResult?> QueryAsync(
        GetBookingByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .Where(x => x.Id == query.BookingId)
            .Select(x => new GetBookingByIdQueryResult
            {
                BookingId = x.Id,
                CustomerId = x.CustomerId,
                VoyageId = x.VoyageId,
                RequestedUnits = x.RequestedUnits,
                Status = ((BookingStatus)x.Status).ToString(),
                CapacityReservationId = x.CapacityReservationId,
                ConfirmedAt = x.ConfirmedAt,
                Version = x.Version,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
