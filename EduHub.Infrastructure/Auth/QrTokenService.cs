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

    public QrPayload Generate(Guid eventId)
    {
        // TODO 4.1: issued = clock.GetUtcNow(); tạo JSON { eventId, issuedAt (giây Unix) }
        // TODO 4.2: body = base64url(JSON); chữ ký = HMACSHA256(secret, body); payload = body + "." + chữ ký
        // TODO 4.3: trả QrPayload(payload, issued + Lifetime)
        throw new NotImplementedException();
    }

    public QrValidation Validate(string payload, out Guid eventId)
    {
        eventId = Guid.Empty;
        // TODO 4.4: tách payload thành 2 phần; sai định dạng => QrValidation.Invalid
        // TODO 4.5: kiểm tra CHỮ KÝ trước (CryptographicOperations.FixedTimeEquals); sai => Invalid
        // TODO 4.6: đọc eventId + issuedAt; quá 15 phút => QrValidation.Expired; còn hạn => Valid
        // Gợi ý: bắt FormatException / JsonException và trả Invalid (payload rác không được làm sập API)
        throw new NotImplementedException();
    }
}
