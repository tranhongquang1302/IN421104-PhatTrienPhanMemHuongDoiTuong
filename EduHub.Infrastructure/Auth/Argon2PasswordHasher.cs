using System.Security.Cryptography;
using System.Text;
using EduHub.Application.Auth;
using Konscious.Security.Cryptography;

namespace EduHub.Infrastructure.Auth;

/// <summary>Băm mật khẩu bằng Argon2id (khuyến nghị của OWASP).</summary>
public class Argon2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;          // 16 byte salt ngẫu nhiên mỗi lần Hash
    private const int HashSize = 32;          // 32 byte (256-bit) đầu ra
    private const int MemorySizeKb = 65536;   // 64 MB
    private const int Iterations = 4;
    private const int Parallelism = 2;

    /// <summary>Trả về base64(salt) + ":" + base64(hash).</summary>
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(Compute(password, salt))}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 2)
            return false;

        var salt = new byte[SaltSize];
        Span<byte> expected = stackalloc byte[HashSize];
        if (!Convert.TryFromBase64String(parts[0], salt, out var saltLength) || saltLength != SaltSize ||
            !Convert.TryFromBase64String(parts[1], expected, out var hashLength) || hashLength != HashSize)
            return false;

        return CryptographicOperations.FixedTimeEquals(Compute(password, salt), expected);
    }

    private static byte[] Compute(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = MemorySizeKb,
            Iterations = Iterations,
            DegreeOfParallelism = Parallelism
        };
        return argon2.GetBytes(HashSize);
    }
}
