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

// TODO [Nhiệm vụ 6] Serilog: builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).Enrich.FromLogContext());
//  và bổ sung mục "Serilog" (MinimumLevel, WriteTo Console) trong appsettings.json.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO [Nhiệm vụ 6] builder.Services.AddProblemDetails();
//                   builder.Services.AddExceptionHandler<GlobalExceptionHandler>();  (cần using EduHub.API.Middleware)

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

// TODO [Nhiệm vụ 6] app.UseSerilogRequestLogging();  rồi  app.UseExceptionHandler();  rồi  app.UseStatusCodePages();
//  (thứ tự này để log request thấy đúng mã trạng thái sau khi lỗi được đổi thành ProblemDetails)

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
