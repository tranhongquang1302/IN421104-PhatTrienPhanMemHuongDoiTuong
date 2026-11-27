using EduHub.Domain.Abstractions;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduHub.Infrastructure.Repositories;

// DbContext đã là Unit of Work. Lớp này chỉ che nó sau IUnitOfWork để lớp Application không phụ thuộc EF Core.
public class EfUnitOfWork(EduHubDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => new EfTransaction(await db.Database.BeginTransactionAsync(ct));

    private sealed class EfTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
