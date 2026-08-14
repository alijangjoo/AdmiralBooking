namespace Admiral.Booking.AppCore.AppServices.Bookings.Commands.Seed;

using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Common.ValueObjects;
using Admiral.Booking.AppCore.Domain.Voyages.Entities;
using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.AppCore.Shared.Bookings.Commands;
using Admiral.Booking.AppCore.Shared.Bookings.Commands.Seed;
using Admiral.Booking.AppCore.Shared.Voyages.Commands;
using MediatR;

public class SeedSampleDataCommandHandler(
    IBookingCommandRepository bookingCommandRepository,
    IVoyageCommandRepository voyageCommandRepository)
    : IRequestHandler<SeedSampleDataCommand, CommandResult<SeedSampleDataCommandResult>>
{
    public async Task<CommandResult<SeedSampleDataCommandResult>> Handle(
        SeedSampleDataCommand request,
        CancellationToken cancellationToken)
    {
        var capacity = request.VoyageCapacity <= 0 ? 2 : request.VoyageCapacity;
        var voyage = Voyage.Create(
            Guid.NewGuid(),
            LocationName.From("Rotterdam"),
            LocationName.From("Singapore"),
            DateTimeOffset.UtcNow.AddDays(14),
            ContainerUnits.From(capacity));

        var bookingA = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), voyage.Id, ContainerUnits.From(capacity));
        var bookingB = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), voyage.Id, ContainerUnits.From(1));

        var closedVoyage = Voyage.Create(
            Guid.NewGuid(),
            LocationName.From("Hamburg"),
            LocationName.From("Shanghai"),
            DateTimeOffset.UtcNow.AddDays(21),
            ContainerUnits.From(10));
        closedVoyage.CloseForBooking();
        var closedBooking = Booking.Create(Guid.NewGuid(), Guid.NewGuid(), closedVoyage.Id, ContainerUnits.From(1));

        await voyageCommandRepository.InsertAsync(voyage);
        await voyageCommandRepository.InsertAsync(closedVoyage);
        await bookingCommandRepository.InsertAsync(bookingA);
        await bookingCommandRepository.InsertAsync(bookingB);
        await bookingCommandRepository.InsertAsync(closedBooking);
        await bookingCommandRepository.CommitAsync();

        return CommandResult<SeedSampleDataCommandResult>.Success(new SeedSampleDataCommandResult
        {
            VoyageId = voyage.Id,
            BookingAId = bookingA.Id,
            BookingBId = bookingB.Id,
            ClosedVoyageId = closedVoyage.Id,
            ClosedVoyageBookingId = closedBooking.Id,
            VoyageCapacity = capacity,
            BookingAUnits = capacity,
            BookingBUnits = 1
        });
    }
}
