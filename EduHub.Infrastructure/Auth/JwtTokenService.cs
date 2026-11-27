using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace EduHub.Infrastructure.Auth;

public class JwtTokenService(IConfiguration config, TimeProvider clock) : ITokenService
{
    public int ExpiresInSeconds => (int.TryParse(config["JwtSettings:ExpiryMinutes"], out var m) ? m : 60) * 60;
    public int RefreshTokenDays => int.TryParse(config["JwtSettings:RefreshTokenDays"], out var d) ? d : 7;

    public string GenerateAccessToken(Account account)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(account.Roles.Select(role => new Claim("role", role)));
        claims.AddRange(account.Claims
            .Where(c => c.Type is not ("role" or "sub" or "email" or "jti" or "iss" or "aud" or "nbf" or "exp" or "iat"))
            .Select(c => new Claim(c.Type, c.Value)));

        var now = clock.GetUtcNow().UtcDateTime;
        var jwt = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            notBefore: now,
            expires: now.AddSeconds(ExpiresInSeconds),
            signingCredentials: new SigningCredentials(ReadSigningKey(config), SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public static SymmetricSecurityKey ReadSigningKey(IConfiguration config)
    {
        var secret = config["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("Cấu hình JwtSettings:Secret qua user-secrets hoặc biến môi trường (tối thiểu 32 byte).");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
