using EduHub.Application.Auth;
using EduHub.Tests.Support;
using Xunit;

namespace EduHub.Tests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewPairAndRevokesOld()
    {
        // TODO 6.4: đăng nhập lấy refresh token A; gọi RefreshCommandHandler với A
        //           => nhận refresh token B khác A, và token A trong store đã IsRevoked
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    [Fact]
    public async Task Refresh_ReusingOldToken_RevokesWholeSession()
    {
        // TODO 6.5: sau khi đổi A lấy B, dùng lại A => UnauthorizedAccessException,
        //           và B cũng bị từ chối (cả phiên bị thu hồi)
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    [Fact]
    public async Task Refresh_AfterExpiry_IsRejected()
    {
        // TODO 6.6: ManualClock.Advance(TimeSpan.FromDays(8)) rồi refresh => UnauthorizedAccessException
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}
