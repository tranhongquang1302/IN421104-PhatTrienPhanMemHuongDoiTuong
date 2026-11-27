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
        // TODO 1.1: sinh salt 16 byte bằng RandomNumberGenerator
        // TODO 1.2: gọi Compute(password, salt) rồi ghép chuỗi base64(salt):base64(hash)
        throw new NotImplementedException();
    }

    public bool Verify(string password, string hash)
    {
        // TODO 1.3: tách chuỗi hash thành salt và hash đã lưu (sai định dạng thì trả false)
        // TODO 1.4: băm lại mật khẩu với CÙNG salt
        // TODO 1.5: so sánh bằng CryptographicOperations.FixedTimeEquals (chống timing attack)
        throw new NotImplementedException();
    }

    private static byte[] Compute(string password, byte[] salt)
    {
        // TODO 1.6: dùng new Argon2id(Encoding.UTF8.GetBytes(password)) với Salt, MemorySize,
        //           Iterations, DegreeOfParallelism theo các hằng số ở trên; trả argon2.GetBytes(HashSize)
        throw new NotImplementedException();
    }
}
