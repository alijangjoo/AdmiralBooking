namespace Admiral.Booking.Endpoints.Api.Extensions.Db;

using Admiral.Booking.Infra.Persistence.Base.Commands;
using Admiral.Booking.Infra.Persistence.Base.Queries;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using Admiral.Booking.Infra.Persistence.MS.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;

public static class DbExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionStrings = configuration.GetSection("ConnectionStrings").Get<DatabaseConnectionStrings>()
            ?? throw new InvalidOperationException("ConnectionStrings section is missing.");

        services.AddDbContext<AppCommandDbContext>(options =>
        {
            options.UseSqlServer(connectionStrings.CommandDb, sql =>
            {
                sql.MigrationsAssembly(typeof(AppCommandDbContext).Assembly.FullName);
            });
        });

        services.AddDbContext<AppQueryDbContext>(options =>
        {
            options.UseSqlServer(connectionStrings.QueryDb);
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var assemblies = GetAssemblies("Admiral.Booking");
        services.Scan(scan => scan.FromAssemblies(assemblies)
            .AddClasses(classes => classes.AssignableToAny(typeof(ICommandRepository<,>), typeof(IQueryRepository)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        return services;
    }

    private static List<Assembly> GetAssemblies(params string[] assemblyName)
    {
        var assemblies = new List<Assembly>();
        var dependencies = DependencyContext.Default!.RuntimeLibraries;
        foreach (var library in dependencies)
        {
            if (IsCandidateCompilationLibrary(library, assemblyName))
                assemblies.Add(Assembly.Load(new AssemblyName(library.Name)));
        }

        return assemblies;
    }

    private static bool IsCandidateCompilationLibrary(RuntimeLibrary compilationLibrary, string[] assemblyName)
        => assemblyName.Any(d => compilationLibrary.Name.Contains(d))
           || compilationLibrary.Dependencies.Any(d => assemblyName.Any(c => d.Name.Contains(c)));
}

public class DatabaseConnectionStrings
{
    public string? CommandDb { get; set; }
    public string? QueryDb { get; set; }
}
