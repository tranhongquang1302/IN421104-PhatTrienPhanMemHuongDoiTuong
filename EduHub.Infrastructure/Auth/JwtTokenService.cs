using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Auth;

public class JwtTokenService(IConfiguration config, TimeProvider clock) : ITokenService
{
    public int ExpiresInSeconds => (int.TryParse(config["JwtSettings:ExpiryMinutes"], out var m) ? m : 60) * 60;
    public int RefreshTokenDays => int.TryParse(config["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;

    public string GenerateAccessToken(Account account)
    {
        // TODO 2.1: đọc JwtSettings:Secret; ném lỗi nếu null hoặc ngắn hơn 32 byte
        // TODO 2.2: tạo claims: sub (account.Id), email, jti (Guid mới)
        //           + mỗi role trong account.Roles thành claim "role"
        //           + mọi claim riêng trong account.Claims (vd: permission = qr.issue)
        // TODO 2.3: tạo JwtSecurityToken (issuer, audience, expires = now + ExpiresInSeconds),
        //           ký HMAC-SHA256 bằng SymmetricSecurityKey, rồi WriteToken
        throw new NotImplementedException();
    }
}
