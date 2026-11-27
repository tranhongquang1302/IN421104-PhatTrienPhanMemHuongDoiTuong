using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfRegistrationRepository(EduHubDbContext db) : IRegistrationRepository
{
    public Task<bool> MemberExistsAsync(Guid memberId, CancellationToken ct = default)
        => db.Members.AnyAsync(m => m.Id == memberId, ct);

    // TODO [Nhiệm vụ 4] Đếm đăng ký còn hiệu lực của sự kiện (Status khác "Cancelled")
    public Task<int> CountActiveByEventIdAsync(Guid eventId, CancellationToken ct = default)
        => throw new NotImplementedException();

    // TODO [Nhiệm vụ 4] Member đã đăng ký sự kiện này chưa (AnyAsync)
    public Task<bool> ExistsAsync(Guid memberId, Guid eventId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Registration?> GetAsync(Guid eventId, Guid registrationId, CancellationToken ct = default)
        => db.Registrations.FirstOrDefaultAsync(r => r.Id == registrationId && r.EventId == eventId, ct);

    // TODO [Nhiệm vụ 4] Danh sách đăng ký của sự kiện kèm Member, sắp theo RegisteredAt
    //  (Join Registrations với Members, rồi ghép thành RegistrationWithMember)
    public Task<IReadOnlyList<RegistrationWithMember>> GetByEventAsync(Guid eventId, CancellationToken ct = default)
        => throw new NotImplementedException();

    public void Add(Registration registration) => db.Registrations.Add(registration);
}
