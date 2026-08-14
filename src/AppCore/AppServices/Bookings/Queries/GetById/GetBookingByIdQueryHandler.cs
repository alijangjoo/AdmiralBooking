namespace Admiral.Booking.AppCore.AppServices.Bookings.Queries.GetById;

using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.AppCore.Shared.Bookings.Queries;
using Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;
using MediatR;

public class GetBookingByIdQueryHandler(IBookingQueryRepository bookingQueryRepository)
    : IRequestHandler<GetBookingByIdQuery, QueryResult<GetBookingByIdQueryResult>>
{
    public async Task<QueryResult<GetBookingByIdQueryResult>> Handle(
        GetBookingByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await bookingQueryRepository.QueryAsync(request, cancellationToken);
        if (result is null)
            return QueryResult<GetBookingByIdQueryResult>.Failure($"Booking '{request.BookingId}' was not found.", "NOT_FOUND");

        return QueryResult<GetBookingByIdQueryResult>.Success(result);
    }
}
