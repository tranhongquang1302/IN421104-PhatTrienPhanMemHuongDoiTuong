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
        var now = clock.GetUtcNow().UtcDateTime;
        var stored = await store.FindByHashAsync(RefreshTokens.Hash(command.RefreshToken), ct);

        if (stored is null)
            throw new UnauthorizedAccessException("Refresh token không hợp lệ.");

        // Token đã bị thu hồi mà vẫn được đem ra dùng lại => nghi bị đánh cắp:
        // thu hồi toàn bộ phiên của người này, buộc đăng nhập lại.
        if (stored.IsRevoked)
        {
            await store.RevokeAllAsync(stored.UserId, now, ct);
            await store.SaveChangesAsync(ct);
            throw new UnauthorizedAccessException("Refresh token đã bị thu hồi.");
        }

        if (stored.IsExpired(now))
            throw new UnauthorizedAccessException("Refresh token đã hết hạn.");

        var account = await accounts.FindByIdAsync(stored.UserId, ct)
            ?? throw new UnauthorizedAccessException("Tài khoản không còn tồn tại.");

        stored.Revoke(now);   // dùng một lần: token cũ chết ngay
        return await LoginCommandHandler.IssueAsync(account, tokens, store, clock, ct);
    }

    public async Task LogoutAsync(RefreshCommand command, CancellationToken ct = default)
    {
        var stored = await store.FindByHashAsync(RefreshTokens.Hash(command.RefreshToken), ct);
        if (stored is null) return;   // đăng xuất luôn "thành công", không lộ token có tồn tại hay không
        stored.Revoke(clock.GetUtcNow().UtcDateTime);
        await store.SaveChangesAsync(ct);
    }
}
