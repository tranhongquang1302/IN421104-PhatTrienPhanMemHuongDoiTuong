using EduHub.Application.Auth;
using EduHub.Tests.Support;
using Xunit;

namespace EduHub.Tests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewPairAndRevokesOld()
    {
        var auth = new TestAuth();
        var original = await auth.SignInAsync();

        var rotated = await auth.Refresh.HandleAsync(new(original.RefreshToken));

        Assert.NotEqual(original.RefreshToken, rotated.RefreshToken);
        Assert.Equal("jwt-token", rotated.AccessToken);
        Assert.Equal(2, auth.Store.Tokens.Count);
        Assert.True(auth.Store.Tokens.Single(t => t.TokenHash == RefreshTokens.Hash(original.RefreshToken)).IsRevoked);
        Assert.False(auth.Store.Tokens.Single(t => t.TokenHash == RefreshTokens.Hash(rotated.RefreshToken)).IsRevoked);
    }

    [Fact]
    public async Task Refresh_ReusingOldToken_RevokesWholeSession()
    {
        var auth = new TestAuth();
        var original = await auth.SignInAsync();
        var separateLogin = await auth.SignInAsync();
        var rotated = await auth.Refresh.HandleAsync(new(original.RefreshToken));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.Refresh.HandleAsync(new(original.RefreshToken)));

        Assert.All(auth.Store.Tokens, token => Assert.True(token.IsRevoked));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.Refresh.HandleAsync(new(rotated.RefreshToken)));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.Refresh.HandleAsync(new(separateLogin.RefreshToken)));
    }

    [Fact]
    public async Task Refresh_AfterExpiry_IsRejected()
    {
        var auth = new TestAuth();
        var original = await auth.SignInAsync();
        auth.Clock.Advance(TimeSpan.FromDays(8));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.Refresh.HandleAsync(new(original.RefreshToken)));
        Assert.False(Assert.Single(auth.Store.Tokens).IsRevoked);
    }

    [Fact]
    public async Task Logout_RevokesTokenAndCanBeRepeated()
    {
        var auth = new TestAuth();
        var original = await auth.SignInAsync();

        await auth.Refresh.LogoutAsync(new(original.RefreshToken));
        await auth.Refresh.LogoutAsync(new(original.RefreshToken));
        await auth.Refresh.LogoutAsync(new("unknown"));

        Assert.True(Assert.Single(auth.Store.Tokens).IsRevoked);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => auth.Refresh.HandleAsync(new(original.RefreshToken)));
    }
}
