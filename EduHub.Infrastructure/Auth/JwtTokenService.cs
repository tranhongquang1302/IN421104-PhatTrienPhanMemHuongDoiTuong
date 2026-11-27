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
        var secret = config["JwtSettings:Secret"];
        if (string.IsNullOrEmpty(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("JwtSettings:Secret phải >= 32 bytes (256-bit).");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, account.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        claims.AddRange(account.Roles.Select(r => new Claim("role", r)));
        claims.AddRange(account.Claims.Select(c => new Claim(c.Type, c.Value)));

        var now = clock.GetUtcNow().UtcDateTime;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var token = new JwtSecurityToken(
            issuer: config["JwtSettings:Issuer"],
            audience: config["JwtSettings:Audience"],
            claims: claims,
            notBefore: now,
            expires: now.AddSeconds(ExpiresInSeconds),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
