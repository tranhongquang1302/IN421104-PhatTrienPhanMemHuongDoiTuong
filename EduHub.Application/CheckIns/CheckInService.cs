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
        if (!await repo.EventExistsAsync(eventId, ct))
            return new(CheckInOutcome.EventNotFound);

        // 1. Chữ ký đúng, chưa hết hạn, và QR thuộc đúng sự kiện này
        var check = qr.Validate(qrPayload, out var payloadEventId);
        if (check == QrValidation.Expired) return new(CheckInOutcome.ExpiredQr);
        if (check != QrValidation.Valid || payloadEventId != eventId) return new(CheckInOutcome.InvalidQr);

        // 2. Member phải đã đăng ký (và chưa huỷ) sự kiện
        var reg = await repo.FindRegistrationAsync(memberId, eventId, ct);
        if (reg is null || reg.Status == "Cancelled") return new(CheckInOutcome.NotRegistered);

        // 3. Chống check-in trùng
        if (await repo.CheckInExistsAsync(reg.Id, ct)) return new(CheckInOutcome.AlreadyCheckedIn);

        // 4. Tạo bản ghi CheckIn
        var checkIn = CheckIn.CreateForRegistration(reg.Id);
        checkIn.Verify(clock.GetUtcNow().UtcDateTime);
        await repo.AddAsync(checkIn, ct);
        return new(CheckInOutcome.Success, checkIn.CheckedInAt);
    }
}
