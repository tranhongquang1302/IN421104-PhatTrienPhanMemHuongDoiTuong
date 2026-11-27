using EduHub.Domain.Entities;

namespace EduHub.Application.CheckIns;

/// <summary>Payload QR đã ký + thời điểm hết hạn.</summary>
public record QrPayload(string Payload, DateTime ExpiresAt);

public enum QrValidation { Valid, Invalid, Expired }

public interface IQrTokenService
{
    /// <summary>Tạo payload QR (HMAC) chứa eventId + issuedAt, sống 15 phút.</summary>
    QrPayload Generate(Guid eventId);
    /// <summary>Kiểm tra chữ ký + hạn; trả eventId trong payload nếu hợp lệ.</summary>
    QrValidation Validate(string payload, out Guid eventId);
}

public interface ICheckInRepository
{
    Task<bool> EventExistsAsync(Guid eventId, CancellationToken ct = default);
    Task<Registration?> FindRegistrationAsync(Guid memberId, Guid eventId, CancellationToken ct = default);
    Task<bool> CheckInExistsAsync(Guid registrationId, CancellationToken ct = default);
    Task AddAsync(CheckIn checkIn, CancellationToken ct = default);
}

public enum CheckInOutcome { Success, EventNotFound, InvalidQr, ExpiredQr, NotRegistered, AlreadyCheckedIn }

public record CheckInResult(CheckInOutcome Outcome, DateTime? CheckedInAt = null);

public class CheckInService(IQrTokenService qr, ICheckInRepository repo, TimeProvider clock)
{
    public QrPayload GenerateQr(Guid eventId) => qr.Generate(eventId);

    public async Task<CheckInResult> CheckInAsync(Guid memberId, Guid eventId, string qrPayload, CancellationToken ct = default)
    {
        // TODO 4.7: sự kiện không tồn tại => EventNotFound
        // TODO 4.8: qr.Validate(...): Expired => ExpiredQr; không Valid HOẶC eventId trong QR khác eventId đang check-in => InvalidQr
        // TODO 4.9: tìm đăng ký của member; không có hoặc đã Cancelled => NotRegistered
        // TODO 4.10: đã check-in rồi => AlreadyCheckedIn
        // TODO 4.11: tạo CheckIn.CreateForRegistration(reg.Id), gọi Verify(clock.GetUtcNow().UtcDateTime), lưu, trả Success
        throw new NotImplementedException();
    }
}
