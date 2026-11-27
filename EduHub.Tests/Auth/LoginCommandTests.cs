using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Moq;
using Xunit;

namespace EduHub.Tests.Auth;

public class LoginCommandTests
{
    private static readonly ManualClock Clock = new(new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero));

    private static (LoginCommandHandler handler, InMemoryRefreshTokenStore store) Build(bool passwordOk)
    {
        var account = TestAccounts.Member();

        var repo = new Mock<IAccountRepository>();
        repo.Setup(r => r.FindByEmailAsync("an@eduhub.local", It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify(It.IsAny<string>(), "salt:hash")).Returns(passwordOk);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.ExpiresInSeconds).Returns(3600);
        tokens.Setup(t => t.RefreshTokenDays).Returns(7);
        tokens.Setup(t => t.GenerateAccessToken(account)).Returns("jwt-token");

        var store = new InMemoryRefreshTokenStore();
        return (new LoginCommandHandler(repo.Object, hasher.Object, tokens.Object, store, Clock), store);
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        var (handler, _) = Build(passwordOk: true);

        var result = await handler.HandleAsync(new LoginCommand("an@eduhub.local", "pw"));

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.Equal(3600, result.ExpiresIn);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        var (handler, store) = Build(passwordOk: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.HandleAsync(new LoginCommand("an@eduhub.local", "wrong")));
        Assert.Empty(store.Tokens);   // đăng nhập hỏng thì không phát refresh token
    }

    [Fact]
    public async Task Login_StoresOnlyHashOfRefreshToken()
    {
        var (handler, store) = Build(passwordOk: true);

        var result = await handler.HandleAsync(new LoginCommand("an@eduhub.local", "pw"));

        var saved = Assert.Single(store.Tokens);
        Assert.NotEqual(result.RefreshToken, saved.TokenHash);
        Assert.Equal(RefreshTokens.Hash(result.RefreshToken), saved.TokenHash);
    }

    // Hasher thật (Argon2id) phải từ chối mật khẩu sai.
    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hasher = new Argon2PasswordHasher();
        var hash = hasher.Hash("password123");

        Assert.True(hasher.Verify("password123", hash));
        Assert.False(hasher.Verify("wrong", hash));
        Assert.NotEqual(hash, hasher.Hash("password123"));   // salt ngẫu nhiên -> hash khác nhau
    }
}
