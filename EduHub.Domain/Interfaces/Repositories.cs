using EduHub.Domain.Entities;

namespace EduHub.Domain.Interfaces;

public interface IClubRepository
{
    Task<IEnumerable<Club>> GetAllAsync(CancellationToken ct = default);
    Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Club club, CancellationToken ct = default);
    /// <summary>Trả false nếu không có CLB để xoá.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IEventRepository
{
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken ct = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Event ev, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IRegistrationRepository
{
    Task<int> CountByEventAsync(Guid eventId, CancellationToken ct = default);
    /// <summary>memberId là Id của người dùng (AppUser) đã đăng nhập.</summary>
    Task<bool> ExistsAsync(Guid eventId, Guid memberId, CancellationToken ct = default);
    Task AddAsync(Registration registration, CancellationToken ct = default);
}
