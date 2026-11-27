using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfClubRepository(EduHubDbContext db) : IClubRepository
{
    public Task<(IReadOnlyList<Club> Items, int TotalCount)> SearchAsync(
        string? search, int skip, int take, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 2 + 3] Dựng IQueryable theo đúng thứ tự: filter -> order -> skip/take
        //  1. Bắt đầu từ db.Clubs.AsNoTracking()
        //  2. Nếu search có nội dung: Where tên chứa từ khóa, không phân biệt hoa thường
        //  3. CountAsync để lấy tổng số bản ghi (trước khi cắt trang)
        //  4. OrderBy(Name).ThenBy(Id).Skip(skip).Take(take).ToListAsync(ct)
        // Không được ToList() rồi mới lọc trong bộ nhớ.
        throw new NotImplementedException();
    }

    public Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Clubs.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
        => db.Clubs.AnyAsync(c => c.Id == id, ct);

    public void Add(Club club) => db.Clubs.Add(club);
    public void Remove(Club club) => db.Clubs.Remove(club);
}
