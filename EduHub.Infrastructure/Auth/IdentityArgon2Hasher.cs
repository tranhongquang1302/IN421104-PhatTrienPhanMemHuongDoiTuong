using EduHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using AppHasher = EduHub.Application.Auth.IPasswordHasher;

namespace EduHub.Infrastructure.Auth;

/// <summary>
/// Cầu nối: cho ASP.NET Core Identity dùng Argon2id thay cho PBKDF2 mặc định.
/// UserManager.CreateAsync(user, password) sẽ gọi HashPassword ở đây.
/// </summary>
public class IdentityArgon2Hasher(AppHasher hasher) : IPasswordHasher<AppUser>
{
    public string HashPassword(AppUser user, string password) => hasher.Hash(password);

    public PasswordVerificationResult VerifyHashedPassword(AppUser user, string hashedPassword, string providedPassword) =>
        hasher.Verify(providedPassword, hashedPassword)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
}
