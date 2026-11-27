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
        // TODO 2.7: băm token nhận được (RefreshTokens.Hash) rồi tìm trong store; không thấy => UnauthorizedAccessException
        // TODO 2.8: token ĐÃ BỊ THU HỒI mà vẫn được dùng lại => nghi bị đánh cắp:
        //           store.RevokeAllAsync(userId, now) + SaveChangesAsync, rồi ném UnauthorizedAccessException
        // TODO 2.9: token hết hạn => UnauthorizedAccessException
        // TODO 2.10: tìm account theo stored.UserId; thu hồi token cũ (stored.Revoke(now));
        //            phát cặp mới bằng LoginCommandHandler.IssueAsync(...)
        throw new NotImplementedException();
    }

    public async Task LogoutAsync(RefreshCommand command, CancellationToken ct = default)
    {
        // TODO 2.11: tìm token theo bản băm; có thì Revoke + SaveChanges; không có cũng KHÔNG báo lỗi
        throw new NotImplementedException();
    }
}
