using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EduHub.Application.CheckIns;
using Microsoft.Extensions.Configuration;
using System.Buffers.Text;

namespace EduHub.Infrastructure.Auth;

/// <summary>
/// Payload QR = base64url(json{eventId, issuedAt}) + "." + base64url(HMAC-SHA256).
/// Sống 15 phút. Ký bằng khoá riêng QrSettings:Secret (nếu không có thì dùng JwtSettings:Secret).
/// </summary>
public class QrTokenService(IConfiguration config, TimeProvider clock) : IQrTokenService
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    private record Body(Guid EventId, long IssuedAt);

    public QrPayload Generate(Guid eventId)
    {
        var issued = clock.GetUtcNow();
        var json = JsonSerializer.SerializeToUtf8Bytes(new Body(eventId, issued.ToUnixTimeSeconds()));
        var body = Base64Url.EncodeToString(json);
        var payload = $"{body}.{Base64Url.EncodeToString(Sign(body))}";
        return new QrPayload(payload, issued.UtcDateTime.Add(Lifetime));
    }

    public QrValidation Validate(string payload, out Guid eventId)
    {
        eventId = Guid.Empty;
        try
        {
            var parts = payload.Split('.');
            if (parts.Length != 2) return QrValidation.Invalid;

            // Kiểm tra chữ ký trước khi tin bất cứ nội dung nào
            var signature = Base64Url.DecodeFromChars(parts[1]);
            if (!CryptographicOperations.FixedTimeEquals(Sign(parts[0]), signature))
                return QrValidation.Invalid;

            var body = JsonSerializer.Deserialize<Body>(Base64Url.DecodeFromChars(parts[0]));
            if (body is null) return QrValidation.Invalid;

            eventId = body.EventId;
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(body.IssuedAt).Add(Lifetime);
            return clock.GetUtcNow() > expiresAt ? QrValidation.Expired : QrValidation.Valid;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException)
        {
            return QrValidation.Invalid;
        }
    }

    private byte[] Sign(string data)
    {
        var secret = config["QrSettings:Secret"] ?? config["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("Thiếu secret để ký QR.");
        return HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(data));
    }
}
