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
    Task<bool> TryAddAsync(CheckIn checkIn, CancellationToken ct = default);
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

        var validation = qr.Validate(qrPayload, out var qrEventId);
        if (validation == QrValidation.Expired)
            return new(CheckInOutcome.ExpiredQr);
        if (validation != QrValidation.Valid || qrEventId != eventId)
            return new(CheckInOutcome.InvalidQr);

        var registration = await repo.FindRegistrationAsync(memberId, eventId, ct);
        if (registration is null || registration.Status == "Cancelled")
            return new(CheckInOutcome.NotRegistered);
        if (await repo.CheckInExistsAsync(registration.Id, ct))
            return new(CheckInOutcome.AlreadyCheckedIn);

        var checkIn = CheckIn.CreateForRegistration(registration.Id);
        checkIn.Verify(clock.GetUtcNow().UtcDateTime);
        if (!await repo.TryAddAsync(checkIn, ct))
            return new(CheckInOutcome.AlreadyCheckedIn);

        return new(CheckInOutcome.Success, checkIn.CheckedInAt);
    }
}
