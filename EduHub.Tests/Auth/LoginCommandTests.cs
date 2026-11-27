using EduHub.Application.Auth;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Xunit;

namespace EduHub.Tests.Auth;

public class LoginCommandTests
{
    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        var auth = new TestAuth();

        var result = await auth.Login.HandleAsync(new($"  {auth.Member.Email.ToUpperInvariant()}  ", "Member@123"));

        Assert.Equal("jwt-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        var stored = Assert.Single(auth.Store.Tokens);
        Assert.Equal(RefreshTokens.Hash(result.RefreshToken), stored.TokenHash);
        Assert.Equal(auth.Member.Id, stored.UserId);
        Assert.Equal(TestAuth.Start.UtcDateTime.AddDays(7), stored.ExpiresAt);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        var auth = new TestAuth(passwordMatches: false);

        var wrongPassword = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.SignInAsync());
        var unknownEmail = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => auth.Login.HandleAsync(new("unknown@eduhub.local", "Member@123")));

        Assert.Equal(wrongPassword.Message, unknownEmail.Message);
        Assert.Empty(auth.Store.Tokens);
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hasher = new Argon2PasswordHasher();
        const string password = "Member@123";
        var firstHash = hasher.Hash(password);
        var secondHash = hasher.Hash(password);

        Assert.NotEqual(firstHash, secondHash);
        Assert.True(hasher.Verify(password, firstHash));
        Assert.False(hasher.Verify("Wrong@123", firstHash));
        Assert.False(hasher.Verify(password, "not:base64"));
        Assert.False(hasher.Verify(password, "YQ==:Yg=="));
    }
}
