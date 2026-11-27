using EduHub.Domain.Abstractions;
using EduHub.Infrastructure.Persistence;

namespace EduHub.Infrastructure.Repositories;

// DbContext đã là Unit of Work. Lớp này chỉ che nó sau IUnitOfWork để lớp Application không phụ thuộc EF Core.
public class EfUnitOfWork(EduHubDbContext db) : IUnitOfWork
{
    // TODO [Nhiệm vụ 2] Gọi db.SaveChangesAsync(ct) và trả kết quả
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => throw new NotImplementedException();
}
