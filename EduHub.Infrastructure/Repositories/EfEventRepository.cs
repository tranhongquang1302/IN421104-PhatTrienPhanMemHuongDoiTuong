using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfEventRepository(EduHubDbContext db) : IEventRepository
{
    public Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
        Guid? clubId, string? search, int skip, int take, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 2 + 3] Tương tự EfClubRepository.SearchAsync, thêm lọc theo clubId.
        //  Sắp xếp theo StartTime (rồi Id) để thứ tự trang ổn định.
        throw new NotImplementedException();
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    public void Add(Event ev) => db.Events.Add(ev);
    public void Remove(Event ev) => db.Events.Remove(ev);
}
