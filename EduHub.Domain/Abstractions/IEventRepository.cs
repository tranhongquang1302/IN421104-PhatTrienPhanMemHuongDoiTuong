using EduHub.Domain.Entities;

namespace EduHub.Domain.Abstractions;

public interface IEventRepository
{
    Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
        Guid? clubId, string? search, int skip, int take, CancellationToken ct = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);   // có tracking, dùng khi cần sửa/xóa
    void Add(Event ev);
    void Remove(Event ev);
}
