using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfEventRepository(EduHubDbContext db) : IEventRepository
{
    public async Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
        Guid? clubId, string? search, int skip, int take, CancellationToken ct = default)
    {
        var query = db.Events.AsNoTracking();
        if (clubId is Guid id)
            query = query.Where(e => e.ClubId == id);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(e => e.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.StartTime).ThenBy(e => e.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return (items, totalCount);
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    public Task<Event?> GetForUpdateAsync(Guid id, CancellationToken ct = default)
        => db.Events.FromSql($"""SELECT * FROM events WHERE "Id" = {id} FOR UPDATE""")
            .SingleOrDefaultAsync(ct);

    public void Add(Event ev) => db.Events.Add(ev);
    public void Remove(Event ev) => db.Events.Remove(ev);
}
