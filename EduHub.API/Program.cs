using System.Text;
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

builder.Services.AddDbContext<EduHubDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- ASP.NET Core Identity: người dùng, vai trò, claim (lưu bằng EF Core) ---
// TODO 3.10: builder.Services.AddIdentityCore<AppUser>(...).AddRoles<AppRole>().AddEntityFrameworkStores<EduHubDbContext>()
//            Luật mật khẩu: tối thiểu 8 ký tự, có chữ hoa, có chữ số; email không được trùng.

builder.Services.AddSingleton(TimeProvider.System);
// TODO 1.7: đăng ký IPasswordHasher -> Argon2PasswordHasher (Singleton)
// TODO 1.8: thay hasher mặc định (PBKDF2) của Identity bằng Argon2id:
//           IPasswordHasher<AppUser> -> IdentityArgon2Hasher (Scoped, đăng ký SAU AddIdentityCore)
builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IQrTokenService, QrTokenService>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RefreshCommandHandler>();
builder.Services.AddScoped<CheckInService>();

// TODO 2.14: AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opts => { ... })
//   - opts.MapInboundClaims = false (giữ tên claim ngắn: sub, email, role)
//   - TokenValidationParameters: kiểm tra Issuer, Audience, Lifetime, chữ ký (SymmetricSecurityKey từ `secret`)
//   - ClockSkew = TimeSpan.Zero; NameClaimType = "sub"; RoleClaimType = "role"
// TODO 3.11: builder.Services.AddAuthorization(Policies.Configure);

// TODO 5.1: CORS — AddCors với policy tên "EduHubWeb": chỉ cho origin trong Cors:AllowedOrigins,
//           method GET/POST/DELETE, header Authorization và Content-Type (KHÔNG dùng AllowAnyOrigin)

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

// TODO 5.2: khi KHÔNG phải môi trường Development: app.UseHsts() và app.UseHttpsRedirection()

app.UseSwagger();
app.UseSwaggerUI();
// TODO 5.3: app.UseCors("EduHubWeb");   // đặt trước Authentication
app.UseAuthentication();   // phải đứng TRƯỚC UseAuthorization
app.UseRateLimiter();      // đặt sau Authentication để giới hạn theo từng người dùng
app.UseAuthorization();
app.MapControllers();
app.Run();
