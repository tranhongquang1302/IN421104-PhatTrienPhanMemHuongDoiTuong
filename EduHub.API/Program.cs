using System.Threading.RateLimiting;
using EduHub.API.Security;
using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Application.CheckIns;
using EduHub.Infrastructure.Auth;
using EduHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IPasswordHasher = EduHub.Application.Auth.IPasswordHasher;   // tránh trùng tên với của Identity

var builder = WebApplication.CreateBuilder(args);

var jwt = builder.Configuration.GetSection("JwtSettings");
var signingKey = JwtTokenService.ReadSigningKey(builder.Configuration);
var clock = TimeProvider.System;
var qrTokens = new QrTokenService(builder.Configuration, clock);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Cấu hình ConnectionStrings:DefaultConnection qua user-secrets hoặc biến môi trường.");

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

builder.Services.AddDbContext<EduHubDbContext>(o => o.UseNpgsql(connectionString));

builder.Services.AddIdentityCore<AppUser>(o =>
{
    o.User.RequireUniqueEmail = true;
    o.Password.RequiredLength = 8;
    o.Password.RequireUppercase = true;
    o.Password.RequireDigit = true;
    o.Password.RequireLowercase = false;
    o.Password.RequireNonAlphanumeric = false;
})
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<EduHubDbContext>();

builder.Services.AddSingleton(clock);
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
builder.Services.AddScoped<IPasswordHasher<AppUser>, IdentityArgon2Hasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IQrTokenService>(qrTokens);
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RefreshCommandHandler>();
builder.Services.AddScoped<CheckInService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.MapInboundClaims = false;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwt["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwt["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
        ClockSkew = TimeSpan.Zero,
        NameClaimType = "sub",
        RoleClaimType = "role"
    };
});
builder.Services.AddAuthorization(Policies.Configure);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Contains("*"))
    throw new InvalidOperationException("Cors:AllowedOrigins phải liệt kê origin cụ thể.");
builder.Services.AddCors(o => o.AddPolicy("EduHubWeb", p => p
    .WithOrigins(corsOrigins)
    .WithMethods("GET", "POST", "DELETE")
    .WithHeaders("Authorization", "Content-Type")));
builder.Services.AddHttpsRedirection(o => o.HttpsPort = builder.Configuration.GetValue<int?>("HTTPS_PORT") ?? 443);

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
        scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>(),
        app.Environment.IsDevelopment());
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("EduHubWeb");
app.UseAuthentication();   // phải đứng TRƯỚC UseAuthorization
app.UseRateLimiter();      // đặt sau Authentication để giới hạn theo từng người dùng
app.UseAuthorization();
app.MapControllers();
app.Run();
