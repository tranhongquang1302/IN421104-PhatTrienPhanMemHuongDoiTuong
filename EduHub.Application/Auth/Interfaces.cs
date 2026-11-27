using EduHub.Application.Abstractions;
using EduHub.Domain.Entities;

namespace EduHub.Application.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenService
{
    /// <summary>Số giây access token còn sống (3600 = 1 giờ).</summary>
    int ExpiresInSeconds { get; }
    /// <summary>Số ngày refresh token còn sống.</summary>
    int RefreshTokenDays { get; }
    /// <summary>Tạo access token (JWT) chứa sub, email, role, các claim riêng và jti.</summary>
    string GenerateAccessToken(Account account);
}

/// <summary>Kho lưu refresh token (chỉ lưu bản băm).</summary>
public interface IRefreshTokenStore
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddAsync(RefreshToken token, CancellationToken ct = default);
    /// <summary>Thu hồi mọi refresh token còn hiệu lực của một người dùng.</summary>
    Task RevokeAllAsync(Guid userId, DateTime utcNow, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
