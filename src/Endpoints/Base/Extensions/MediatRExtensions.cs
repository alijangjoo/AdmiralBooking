namespace Admiral.Booking.Endpoints.Base.Extensions;

using Admiral.Booking.Endpoints.Base.Validations;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class MediatRExtensions
{
    public static IServiceCollection AddMediatRServices(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] additionalAssemblies)
    {
        var assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };
        if (additionalAssemblies is not null)
            assemblies.AddRange(additionalAssemblies);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies.ToArray()));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddValidatorsFromAssemblies(assemblies.ToArray(), ServiceLifetime.Scoped);
        return services;
    }
}
