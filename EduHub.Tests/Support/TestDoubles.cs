using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Domain.Entities;
using Moq;

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
    private readonly SemaphoreSlim _refreshLock = new(1);
    public List<RefreshToken> Tokens { get; } = [];

    public async Task<TResult> WithTokenAsync<TResult>(
        string tokenHash, Func<RefreshToken?, Task<TResult>> action, CancellationToken ct = default)
    {
        await _refreshLock.WaitAsync(ct);
        try
        {
            return await action(Tokens.SingleOrDefault(t => t.TokenHash == tokenHash));
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        Tokens.Add(token);
        return Task.CompletedTask;
    }

    public Task RevokeAllAsync(Guid userId, DateTime utcNow, CancellationToken ct = default)
    {
        foreach (var t in Tokens.Where(t => t.UserId == userId && t.IsActive(utcNow))) t.Revoke(utcNow);
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

internal sealed class TestAuth
{
    public static readonly DateTimeOffset Start = new(2026, 10, 12, 8, 0, 0, TimeSpan.Zero);
    public Account Member { get; } = TestAccounts.Member();
    public ManualClock Clock { get; } = new(Start);
    public InMemoryRefreshTokenStore Store { get; } = new();
    public LoginCommandHandler Login { get; }
    public RefreshCommandHandler Refresh { get; }

    public TestAuth(bool passwordMatches = true)
    {
        var accounts = new Mock<IAccountRepository>();
        accounts.Setup(r => r.FindByEmailAsync(Member.Email, It.IsAny<CancellationToken>())).ReturnsAsync(Member);
        accounts.Setup(r => r.FindByIdAsync(Member.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Member);

        var passwords = new Mock<IPasswordHasher>();
        passwords.Setup(h => h.Verify("Member@123", Member.PasswordHash)).Returns(passwordMatches);

        var tokens = new Mock<ITokenService>();
        tokens.SetupGet(s => s.ExpiresInSeconds).Returns(3600);
        tokens.SetupGet(s => s.RefreshTokenDays).Returns(7);
        tokens.Setup(s => s.GenerateAccessToken(Member)).Returns("jwt-token");

        Login = new(accounts.Object, passwords.Object, tokens.Object, Store, Clock);
        Refresh = new(accounts.Object, tokens.Object, Store, Clock);
    }

    public Task<LoginResult> SignInAsync() => Login.HandleAsync(new(Member.Email, "Member@123"));
}
