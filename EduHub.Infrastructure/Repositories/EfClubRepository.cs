using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfClubRepository(EduHubDbContext db) : IClubRepository
{
    public async Task<(IReadOnlyList<Club> Items, int TotalCount)> SearchAsync(
        string? search, int skip, int take, CancellationToken ct = default)
    {
        var query = db.Clubs.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(c => c.Name).ThenBy(c => c.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return (items, totalCount);
    }

    public Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Clubs.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => db.Clubs.AnyAsync(c => c.Id == id, ct);

    public void Add(Club club) => db.Clubs.Add(club);
    public void Remove(Club club) => db.Clubs.Remove(club);
}
