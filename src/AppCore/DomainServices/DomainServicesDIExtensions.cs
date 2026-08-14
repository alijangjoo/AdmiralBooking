namespace Admiral.Booking.AppCore.DomainServices;

using Admiral.Booking.AppCore.DomainServices.Bookings;
using Admiral.Booking.AppCore.Shared.Bookings.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DomainServicesDIExtensions
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingConfirmationService, BookingConfirmationService>();
        return services;
    }
}
