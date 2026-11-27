using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfRegistrationRepository(EduHubDbContext db) : IRegistrationRepository
{
    public Task<bool> MemberExistsAsync(Guid memberId, CancellationToken ct = default)
        => db.Members.AnyAsync(m => m.Id == memberId, ct);

    public Task<int> CountActiveByEventIdAsync(Guid eventId, CancellationToken ct = default)
        => db.Registrations.CountAsync(r => r.EventId == eventId && r.Status != "Cancelled", ct);

    public Task<bool> ExistsAsync(Guid memberId, Guid eventId, CancellationToken ct = default)
        => db.Registrations.AnyAsync(r => r.MemberId == memberId && r.EventId == eventId, ct);

    public Task<Registration?> GetAsync(Guid eventId, Guid registrationId, CancellationToken ct = default)
        => db.Registrations.FirstOrDefaultAsync(r => r.Id == registrationId && r.EventId == eventId, ct);

    public async Task<IReadOnlyList<RegistrationWithMember>> GetByEventAsync(
        Guid eventId, CancellationToken ct = default)
    {
        var query = from registration in db.Registrations.AsNoTracking()
                    join member in db.Members.AsNoTracking() on registration.MemberId equals member.Id
                    where registration.EventId == eventId
                    orderby registration.RegisteredAt, registration.Id
                    select new RegistrationWithMember(registration, member);

        return await query.ToListAsync(ct);
    }

    public void Add(Registration registration) => db.Registrations.Add(registration);
}
