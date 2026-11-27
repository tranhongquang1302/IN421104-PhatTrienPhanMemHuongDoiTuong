using System.Text;
using System.Threading.RateLimiting;
using EduHub.API.Security;
using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Application.CheckIns;
using EduHub.Application.Clubs;
using EduHub.Application.Events;
using EduHub.Application.Registrations;
using EduHub.Domain.Interfaces;
using EduHub.Infrastructure.Auth;
using EduHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IPasswordHasher = EduHub.Application.Auth.IPasswordHasher;   // tránh trùng tên với của Identity

var builder = WebApplication.CreateBuilder(args);

// Secret đọc từ user-secrets / biến môi trường — KHÔNG ghi vào appsettings.json
var jwt = builder.Configuration.GetSection("JwtSettings");
var secret = jwt["Secret"];
if (string.IsNullOrEmpty(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
    throw new InvalidOperationException(
        "Thiếu JwtSettings:Secret (>= 32 bytes). Chạy: dotnet user-secrets set \"JwtSettings:Secret\" \"<chuỗi ngẫu nhiên dài>\" --project EduHub.API");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Nút "Authorize" trên Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Dán access token (không cần gõ chữ Bearer)"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = []
    });
});

// Có connection string -> PostgreSQL; không có -> EF Core in-memory (dev/test, không cần PostgreSQL)
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<EduHubDbContext>(o =>
{
    if (string.IsNullOrWhiteSpace(conn)) o.UseInMemoryDatabase("eduhub");
    else o.UseNpgsql(conn);
});

// --- ASP.NET Core Identity: người dùng, vai trò, claim (lưu bằng EF Core) ---
builder.Services.AddIdentityCore<AppUser>(o =>
    {
        o.Password.RequiredLength = 8;
        o.Password.RequireDigit = true;
        o.Password.RequireUppercase = true;
        o.User.RequireUniqueEmail = true;
    })
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<EduHubDbContext>();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
// Thay hasher mặc định (PBKDF2) của Identity bằng Argon2id
builder.Services.AddScoped<IPasswordHasher<AppUser>, IdentityArgon2Hasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IQrTokenService, QrTokenService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RefreshCommandHandler>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<IClubRepository, ClubRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<GetClubsQueryHandler>();
builder.Services.AddScoped<GetClubByIdQueryHandler>();
builder.Services.AddScoped<CreateClubCommandHandler>();
builder.Services.AddScoped<CreateEventCommandHandler>();
builder.Services.AddScoped<RegisterForEventCommandHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.MapInboundClaims = false;   // giữ nguyên tên claim ngắn: sub, email, role
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero,       // hết hạn là 401 ngay
            NameClaimType = "sub",
            RoleClaimType = "role"           // để [Authorize(Roles = ...)] đọc đúng claim "role"
        };
    });
builder.Services.AddAuthorization(Policies.Configure);

// --- CORS: chỉ cho phép web front-end đã biết gọi API từ trình duyệt ---
builder.Services.AddCors(o => o.AddPolicy("EduHubWeb", p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .WithMethods("GET", "POST", "DELETE")
    .WithHeaders("Authorization", "Content-Type")));

// --- Rate limiting: chống dò mật khẩu và thử QR hàng loạt ---
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy("auth", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy("checkin", ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.User.FindFirst("sub")?.Value ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 20, Window = TimeSpan.FromMinutes(1) }));
});

var app = builder.Build();

// Demo: tạo bảng + dữ liệu mẫu khi chạy (thực tế dùng EF migrations)
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<EduHubDbContext>(),
        scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),
        scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>());
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();               // bảo trình duyệt luôn dùng HTTPS với site này
    app.UseHttpsRedirection();   // http:// tự chuyển sang https://
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("EduHubWeb");
app.UseAuthentication();   // phải đứng TRƯỚC UseAuthorization
app.UseRateLimiter();      // đặt sau Authentication để giới hạn theo từng người dùng
app.UseAuthorization();
app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy" }));   // cho healthcheck của Docker
app.MapControllers();
app.Run();

// Cho WebApplicationFactory<Program> dùng trong integration test
public partial class Program;
