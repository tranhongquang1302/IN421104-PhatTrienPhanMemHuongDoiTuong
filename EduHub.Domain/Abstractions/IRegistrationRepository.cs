using EduHub.Domain.Entities;

namespace EduHub.Domain.Abstractions;

// Kết quả ghép Registration với Member để trả danh sách người đăng ký
public record RegistrationWithMember(Registration Registration, Member Member);

public interface IRegistrationRepository
{
    Task<bool> MemberExistsAsync(Guid memberId, CancellationToken ct = default);
    Task<int> CountActiveByEventIdAsync(Guid eventId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid memberId, Guid eventId, CancellationToken ct = default);
    Task<Registration?> GetAsync(Guid eventId, Guid registrationId, CancellationToken ct = default);
    Task<IReadOnlyList<RegistrationWithMember>> GetByEventAsync(Guid eventId, CancellationToken ct = default);
    void Add(Registration registration);
}
