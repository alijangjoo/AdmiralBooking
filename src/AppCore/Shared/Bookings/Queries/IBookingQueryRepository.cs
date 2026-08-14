namespace Admiral.Booking.AppCore.Shared.Bookings.Queries;

using Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;
using Admiral.Booking.Infra.Persistence.Base.Queries;

public interface IBookingQueryRepository : IQueryRepository
{
    Task<GetBookingByIdQueryResult?> QueryAsync(GetBookingByIdQuery query, CancellationToken cancellationToken = default);
}
