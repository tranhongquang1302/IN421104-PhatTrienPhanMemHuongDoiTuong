using EduHub.Domain.Entities;

namespace EduHub.Domain.Abstractions;

public interface IClubRepository
{
    // Tìm theo tên (không phân biệt hoa thường), sắp xếp, rồi mới Skip/Take trong SQL
    Task<(IReadOnlyList<Club> Items, int TotalCount)> SearchAsync(string? search, int skip, int take, CancellationToken ct = default);
    Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default);   // có tracking, dùng khi cần sửa/xóa
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    void Add(Club club);
    void Remove(Club club);
}
