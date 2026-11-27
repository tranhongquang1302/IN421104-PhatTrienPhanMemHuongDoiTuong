using System.Security.Cryptography;
using System.Text;
using EduHub.Application.Auth;
using Konscious.Security.Cryptography;

namespace EduHub.Infrastructure.Auth;

/// <summary>Argon2id password hasher — OWASP recommendation 2023+.</summary>
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
        var hash = Compute(password, salt);
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split(':');
        if (parts.Length != 2) return false;
        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);
            var actual = Compute(password, salt);
            // So sánh thời gian hằng số — chống timing attack
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
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
