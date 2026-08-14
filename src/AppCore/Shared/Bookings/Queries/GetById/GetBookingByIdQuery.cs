namespace Admiral.Booking.AppCore.Shared.Bookings.Queries.GetById;

using Admiral.Booking.AppCore.Shared.Base;
using MediatR;

public class GetBookingByIdQuery : IRequest<QueryResult<GetBookingByIdQueryResult>>
{
    public Guid BookingId { get; set; }
}
