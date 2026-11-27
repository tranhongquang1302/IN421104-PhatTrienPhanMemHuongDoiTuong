namespace EduHub.Domain.Entities;

/// <summary>
/// Refresh token. DB chỉ lưu bản băm SHA-256 của token, không lưu token gốc
/// (lộ DB cũng không dùng được). Mỗi lần làm mới, token cũ bị thu hồi (xoay vòng token).
/// </summary>
public class RefreshToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTime utcNow, TimeSpan lifetime) =>
        new() { UserId = userId, TokenHash = tokenHash, CreatedAt = utcNow, ExpiresAt = utcNow.Add(lifetime) };

    public bool IsRevoked => RevokedAt is not null;
    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;
    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public void Revoke(DateTime utcNow) => RevokedAt ??= utcNow;
}
