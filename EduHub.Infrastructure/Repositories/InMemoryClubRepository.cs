using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Infrastructure.Repositories;

/// <summary>
/// Kho CLB lưu tạm trong bộ nhớ — dùng cho Chương 3 khi chưa có database.
/// Chương 4 sẽ thay bằng bản dùng EF Core và PostgreSQL.
/// </summary>
public class InMemoryClubRepository : IClubRepository
{
    // static: repository đăng ký Scoped (mỗi request một bản mới) nhưng dữ liệu phải sống suốt vòng đời ứng dụng.
    private static readonly List<Club> _clubs = new();

    public Task<IEnumerable<Club>> GetAllAsync(CancellationToken ct = default)
    {
        // TODO 1a: trả về bản sao danh sách _clubs (dùng Task.FromResult)
        throw new NotImplementedException();
    }

    public Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // TODO 1b: tìm club theo Id; không thấy thì trả null, KHÔNG throw
        throw new NotImplementedException();
    }

    public Task AddAsync(Club club, CancellationToken ct = default)
    {
        // TODO 1c: thêm club vào _clubs
        throw new NotImplementedException();
    }

    public Task UpdateAsync(Club club, CancellationToken ct = default)
    {
        // TODO 1d: tìm club cũ theo Id và thay bằng club mới; không thấy thì throw KeyNotFoundException
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // TODO 1e: xoá club theo Id; không thấy thì throw KeyNotFoundException
        throw new NotImplementedException();
    }
}
