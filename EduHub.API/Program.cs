using EduHub.API.Middleware;
using EduHub.Application.Clubs;
using EduHub.Application.Events;
using EduHub.Application.Registrations;
using EduHub.Application.Reports;
using EduHub.Domain.Abstractions;
using EduHub.Infrastructure.Persistence;
using EduHub.Infrastructure.Reports;
using EduHub.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, log) => log
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduHubDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();
builder.Services.AddScoped<IClubRepository, EfClubRepository>();
builder.Services.AddScoped<IEventRepository, EfEventRepository>();
builder.Services.AddScoped<IRegistrationRepository, EfRegistrationRepository>();
builder.Services.AddScoped<ClubService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<GetEventsQueryHandler>();
builder.Services.AddScoped<RegisterForEventCommandHandler>();
builder.Services.AddScoped<GetEventRegistrationsQueryHandler>();
builder.Services.AddScoped<CancelRegistrationCommandHandler>();
builder.Services.AddScoped<IEventReportQuery, DapperEventReportQuery>();

var app = builder.Build();

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Cần cấu hình ConnectionStrings__DefaultConnection.");

if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<EduHubDbContext>();
    var ct = app.Lifetime.ApplicationStopping;
    await db.Database.MigrateAsync(ct);
    await db.SeedAsync(ct);
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnostics, context) =>
        diagnostics.Set("TraceId", context.TraceIdentifier);
});
app.UseExceptionHandler();
app.UseStatusCodePages(context => Results.Problem(
    statusCode: context.HttpContext.Response.StatusCode,
    instance: context.HttpContext.Request.Path,
    extensions: new Dictionary<string, object?> { ["traceId"] = context.HttpContext.TraceIdentifier })
    .ExecuteAsync(context.HttpContext));

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
