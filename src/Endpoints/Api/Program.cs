using Admiral.Booking.AppCore.AppServices;
using Admiral.Booking.AppCore.AppServices.OutboxMessages;
using Admiral.Booking.AppCore.DomainServices;
using Admiral.Booking.AppCore.Shared;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Options;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Services;
using Admiral.Booking.Endpoints.Api.Extensions.Db;
using Admiral.Booking.Endpoints.Api.Workers;
using Admiral.Booking.Endpoints.Base.Extensions;
using Admiral.Booking.Endpoints.Base.Middlewares;
using Admiral.Booking.Infra.Persistence.MS.Commands;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console();
});

builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection(OutboxOptions.SectionName));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<AppCommandDbContext>();
builder.Services.AddDatabase(builder.Configuration).AddRepositories();
builder.Services.AddDomainServices();
builder.Services.AddMediatRServices(
    builder.Configuration,
    typeof(Program).Assembly,
    typeof(IAppLayerMediatRMarkerService).Assembly,
    typeof(ISharedLayerMediatRMarkerService).Assembly);
builder.Services.AddSingleton<PublicationFailureGate>();
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddScoped<IIntegrationEventPublisher, ConfigurableIntegrationEventPublisher>();
builder.Services.AddScoped<IBookingConfirmedConsumer, IdempotentBookingConfirmedConsumer>();
builder.Services.AddHostedService<OutboxPublisherHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppCommandDbContext>();
    await db.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<BusinessRuleExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
