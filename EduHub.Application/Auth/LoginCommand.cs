using System.Security.Cryptography;
using System.Text;
using EduHub.Application.Abstractions;
using EduHub.Domain.Entities;

namespace EduHub.Application.Auth;

public record LoginCommand(string Email, string Password);

/// <summary>Kết quả đăng nhập — JSON: { accessToken, refreshToken, expiresIn, tokenType }.</summary>
public record LoginResult(string AccessToken, string RefreshToken, int ExpiresIn, string TokenType = "Bearer");

/// <summary>Tạo refresh token ngẫu nhiên và băm nó trước khi lưu.</summary>
public static class RefreshTokens
{
    public static string NewToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}

public class LoginCommandHandler(
    IAccountRepository accounts,
    IPasswordHasher hasher,
    ITokenService tokens,
    IRefreshTokenStore refreshTokens,
    TimeProvider clock)
{
    public async Task<LoginResult> HandleAsync(LoginCommand command, CancellationToken ct = default)
    {
        var account = await accounts.FindByEmailAsync(command.Email.Trim().ToLowerInvariant(), ct);

        // Email không tồn tại hay sai mật khẩu đều trả CÙNG một lỗi,
        // để kẻ tấn công không dò ra được email nào có trong hệ thống.
        if (account is null || !hasher.Verify(command.Password, account.PasswordHash))
            throw new UnauthorizedAccessException("Email hoặc mật khẩu không đúng.");

        return await IssueAsync(account, tokens, refreshTokens, clock, ct);
    }

    internal static async Task<LoginResult> IssueAsync(
        Account account, ITokenService tokens, IRefreshTokenStore store, TimeProvider clock, CancellationToken ct)
    {
        var raw = RefreshTokens.NewToken();
        await store.AddAsync(
            RefreshToken.Create(account.Id, RefreshTokens.Hash(raw), clock.GetUtcNow().UtcDateTime,
                TimeSpan.FromDays(tokens.RefreshTokenDays)), ct);
        await store.SaveChangesAsync(ct);
        return new LoginResult(tokens.GenerateAccessToken(account), raw, tokens.ExpiresInSeconds);
    }
}
