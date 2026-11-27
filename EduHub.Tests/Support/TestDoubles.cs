using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Domain.Entities;

namespace EduHub.Tests.Support;

/// <summary>Đồng hồ giả: test điều khiển được thời gian (hết hạn token, QR).</summary>
public class ManualClock(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now = _now.Add(by);
}

/// <summary>Kho refresh token trong bộ nhớ, thay cho DB.</summary>
public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    public List<RefreshToken> Tokens { get; } = [];

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default) =>
        Task.FromResult(Tokens.FirstOrDefault(t => t.TokenHash == tokenHash));

    public Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        Tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task RevokeAllAsync(Guid userId, DateTime utcNow, CancellationToken ct = default)
    {
        foreach (var t in Tokens.Where(t => t.UserId == userId)) t.Revoke(utcNow);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public static class TestAccounts
{
    public static Account Member(string email = "an@eduhub.local") => new(
        Guid.NewGuid(), email, "An", "salt:hash",
        [Roles.Member], [new UserClaim(AppClaims.Permission, AppClaims.QrIssue)]);
}
