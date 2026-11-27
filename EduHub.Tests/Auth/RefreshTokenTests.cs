using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Tests.Support;
using Moq;
using Xunit;

namespace EduHub.Tests.Auth;

public class RefreshTokenTests
{
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero));
    private readonly InMemoryRefreshTokenStore _store = new();
    private readonly Account _account = TestAccounts.Member();
    private readonly RefreshCommandHandler _refresh;
    private readonly LoginCommandHandler _login;

    public RefreshTokenTests()
    {
        var repo = new Mock<IAccountRepository>();
        repo.Setup(r => r.FindByEmailAsync(_account.Email, It.IsAny<CancellationToken>())).ReturnsAsync(_account);
        repo.Setup(r => r.FindByIdAsync(_account.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_account);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.ExpiresInSeconds).Returns(3600);
        tokens.Setup(t => t.RefreshTokenDays).Returns(7);
        tokens.Setup(t => t.GenerateAccessToken(It.IsAny<Account>())).Returns(() => "jwt-" + Guid.NewGuid());

        _login = new LoginCommandHandler(repo.Object, hasher.Object, tokens.Object, _store, _clock);
        _refresh = new RefreshCommandHandler(repo.Object, tokens.Object, _store, _clock);
    }

    private async Task<string> LoginAsync() =>
        (await _login.HandleAsync(new LoginCommand(_account.Email, "pw"))).RefreshToken;

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewPairAndRevokesOld()
    {
        var first = await LoginAsync();

        var second = await _refresh.HandleAsync(new RefreshCommand(first));

        Assert.NotEqual(first, second.RefreshToken);
        Assert.True(_store.Tokens.Single(t => t.TokenHash == RefreshTokens.Hash(first)).IsRevoked);
    }

    [Fact]
    public async Task Refresh_ReusingOldToken_RevokesWholeSession()
    {
        var first = await LoginAsync();
        var second = await _refresh.HandleAsync(new RefreshCommand(first));

        // Token cũ bị dùng lại => bị từ chối, và token mới cũng bị thu hồi
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _refresh.HandleAsync(new RefreshCommand(first)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _refresh.HandleAsync(new RefreshCommand(second.RefreshToken)));
    }

    [Fact]
    public async Task Refresh_AfterExpiry_IsRejected()
    {
        var token = await LoginAsync();
        _clock.Advance(TimeSpan.FromDays(8));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _refresh.HandleAsync(new RefreshCommand(token)));
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_IsRejected() =>
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _refresh.HandleAsync(new RefreshCommand("khong-co-that")));

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var token = await LoginAsync();

        await _refresh.LogoutAsync(new RefreshCommand(token));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _refresh.HandleAsync(new RefreshCommand(token)));
    }
}
