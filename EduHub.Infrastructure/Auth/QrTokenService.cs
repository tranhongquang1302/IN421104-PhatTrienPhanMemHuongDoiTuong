using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EduHub.Application.CheckIns;
using Microsoft.Extensions.Configuration;

namespace EduHub.Infrastructure.Auth;

/// <summary>
/// Payload QR = base64url(json{eventId, issuedAt}) + "." + base64url(HMAC-SHA256).
/// Sống 15 phút. Ký bằng QrSettings:Secret (nếu không có thì dùng JwtSettings:Secret).
/// </summary>
public class QrTokenService(IConfiguration config, TimeProvider clock) : IQrTokenService
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly byte[] _key = ReadKey(config);

    private sealed record Body(Guid EventId, long IssuedAt);

    public QrPayload Generate(Guid eventId)
    {
        var issuedAt = clock.GetUtcNow().ToUnixTimeSeconds();
        var body = Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(new Body(eventId, issuedAt), JsonOptions));
        var signature = Base64Url.EncodeToString(Sign(body));
        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(issuedAt).Add(Lifetime);
        return new QrPayload($"{body}.{signature}", expiresAt.UtcDateTime);
    }

    public QrValidation Validate(string payload, out Guid eventId)
    {
        eventId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(payload) || payload.Length > 256)
            return QrValidation.Invalid;

        var parts = payload.Split('.');
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length != 43)
            return QrValidation.Invalid;

        try
        {
            var signature = Base64Url.DecodeFromChars(parts[1]);
            if (!CryptographicOperations.FixedTimeEquals(Sign(parts[0]), signature))
                return QrValidation.Invalid;

            var body = JsonSerializer.Deserialize<Body>(Base64Url.DecodeFromChars(parts[0]), JsonOptions);
            if (body is null || body.EventId == Guid.Empty || body.IssuedAt <= 0)
                return QrValidation.Invalid;

            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(body.IssuedAt);
            var now = clock.GetUtcNow();
            if (issuedAt > now)
                return QrValidation.Invalid;
            if (now >= issuedAt.Add(Lifetime))
                return QrValidation.Expired;

            eventId = body.EventId;
            return QrValidation.Valid;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentOutOfRangeException)
        {
            return QrValidation.Invalid;
        }
    }

    private byte[] Sign(string body) => HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(body));

    private static byte[] ReadKey(IConfiguration config)
    {
        var secret = config["QrSettings:Secret"];
        if (string.IsNullOrEmpty(secret))
            return JwtTokenService.ReadSigningKey(config).Key;
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32)
            throw new InvalidOperationException("QrSettings:Secret phải có ít nhất 32 byte.");
        return Encoding.UTF8.GetBytes(secret);
    }
}
