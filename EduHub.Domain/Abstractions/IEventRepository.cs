using EduHub.Domain.Entities;

namespace EduHub.Domain.Abstractions;

public interface IEventRepository
{
    Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
        Guid? clubId, string? search, int skip, int take, CancellationToken ct = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);   // có tracking, dùng khi cần sửa/xóa
    // Khóa dòng sự kiện trong giao dịch hiện hành trước khi kiểm tra số lượt đăng ký.
    Task<Event?> GetForUpdateAsync(Guid id, CancellationToken ct = default);
    void Add(Event ev);
    void Remove(Event ev);
}
