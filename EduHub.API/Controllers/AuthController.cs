using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    LoginCommandHandler login,
    RefreshCommandHandler refresh,
    IAccountRepository accounts) : ControllerBase
{
    /// <summary>Đăng ký thành viên mới (vai trò Member). Identity kiểm tra độ mạnh mật khẩu.</summary>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await accounts.CreateMemberAsync(request.Email, request.DisplayName, request.Password, ct);
        if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
        return Created("/api/auth/me", new { result.Account!.Id, result.Account.Email });
    }

    /// <summary>Đăng nhập — trả { accessToken, refreshToken, expiresIn, tokenType }.</summary>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]   // chống dò mật khẩu: tối đa 10 lần/phút/IP
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        // TODO 2.12: gọi login.HandleAsync(new LoginCommand(...)); trả Ok(kết quả);
        //            bắt UnauthorizedAccessException => Unauthorized(new { error = ex.Message })
        throw new NotImplementedException();
    }

    /// <summary>Đổi refresh token lấy cặp token mới. Refresh token cũ hết tác dụng.</summary>
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        // TODO 2.13: tương tự Login, nhưng gọi refresh.HandleAsync(new RefreshCommand(...))
        throw new NotImplementedException();
    }

    /// <summary>Đăng xuất: thu hồi refresh token.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await refresh.LogoutAsync(new RefreshCommand(request.RefreshToken), ct);
        return NoContent();
    }

    /// <summary>Xem thông tin đăng nhập hiện tại (các claim trong access token).</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me() => Ok(new
    {
        id = User.FindFirst("sub")?.Value,
        email = User.FindFirst("email")?.Value,
        roles = User.FindAll("role").Select(c => c.Value),
        permissions = User.FindAll("permission").Select(c => c.Value)
    });
}

public record RegisterRequest(string Email, string DisplayName, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
