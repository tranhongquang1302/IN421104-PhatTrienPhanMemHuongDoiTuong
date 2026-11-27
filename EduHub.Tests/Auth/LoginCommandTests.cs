using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Moq;
using Xunit;

namespace EduHub.Tests.Auth;

public class LoginCommandTests
{
    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        // TODO 6.1: Arrange: mock IAccountRepository (trả TestAccounts.Member()), IPasswordHasher.Verify = true,
        //           ITokenService (ExpiresInSeconds = 3600, RefreshTokenDays = 7, GenerateAccessToken trả "jwt-token"),
        //           InMemoryRefreshTokenStore, ManualClock
        // Act: gọi LoginCommandHandler.HandleAsync
        // Assert: AccessToken và RefreshToken không rỗng
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        // TODO 6.2: IPasswordHasher.Verify trả false => Assert.ThrowsAsync<UnauthorizedAccessException>
        //           và store không được chứa refresh token nào
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // TODO 6.3 (test MỚI, tự viết): dùng Argon2PasswordHasher thật:
        //   - Verify đúng mật khẩu => true; sai mật khẩu => false
        //   - Hash cùng một mật khẩu hai lần => hai chuỗi khác nhau (salt ngẫu nhiên)
        throw new NotImplementedException();
    }
}
