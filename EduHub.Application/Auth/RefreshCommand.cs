using EduHub.Application.Abstractions;

namespace EduHub.Application.Auth;

public record RefreshCommand(string RefreshToken);

/// <summary>Đổi refresh token lấy cặp token mới (xoay vòng) hoặc đăng xuất (thu hồi).</summary>
public class RefreshCommandHandler(
    IAccountRepository accounts,
    ITokenService tokens,
    IRefreshTokenStore store,
    TimeProvider clock)
{
    public async Task<LoginResult> HandleAsync(RefreshCommand command, CancellationToken ct = default)
    {
        var result = await store.WithTokenAsync<LoginResult?>(RefreshTokens.Hash(command.RefreshToken), async stored =>
        {
            if (stored is null)
                return null;

            var now = clock.GetUtcNow().UtcDateTime;
            if (stored.IsRevoked)
            {
                await store.RevokeAllAsync(stored.UserId, now, ct);
                await store.SaveChangesAsync(ct);
                return null;
            }

            if (stored.IsExpired(now))
                return null;

            var account = await accounts.FindByIdAsync(stored.UserId, ct);
            if (account is null)
                return null;

            stored.Revoke(now);
            return await LoginCommandHandler.IssueAsync(account, tokens, store, clock, ct);
        }, ct);

        // Trả lỗi sau khi giao dịch đã lưu việc thu hồi do token bị dùng lại.
        return result ?? throw new UnauthorizedAccessException("Refresh token không hợp lệ.");
    }

    public Task LogoutAsync(RefreshCommand command, CancellationToken ct = default) =>
        store.WithTokenAsync(RefreshTokens.Hash(command.RefreshToken), async stored =>
        {
            if (stored is null || stored.IsRevoked)
                return false;

            stored.Revoke(clock.GetUtcNow().UtcDateTime);
            await store.SaveChangesAsync(ct);
            return true;
        }, ct);
}
