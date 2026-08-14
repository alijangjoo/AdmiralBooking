namespace Admiral.Booking.AppCore.Shared.Bookings.Commands.Seed;

using Admiral.Booking.AppCore.Shared.Base;
using MediatR;

public class SeedSampleDataCommand : IRequest<CommandResult<SeedSampleDataCommandResult>>
{
    public int VoyageCapacity { get; set; } = 2;
}
