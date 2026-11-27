namespace EduHub.Domain.Entities;

/// <summary>Một lượt điểm danh của một đăng ký. QR không lưu trong DB: nó là token có chữ ký, có hạn.</summary>
public class CheckIn
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid RegistrationId { get; set; }
    public DateTime? CheckedInAt { get; private set; }
    public bool IsVerified { get; private set; }

    private CheckIn() { }

    public static CheckIn CreateForRegistration(Guid registrationId) =>
        new() { RegistrationId = registrationId };

    public void Verify(DateTime utcNow)
    {
        if (IsVerified) throw new InvalidOperationException("Already checked in");
        IsVerified = true;
        CheckedInAt = utcNow;
    }
}
