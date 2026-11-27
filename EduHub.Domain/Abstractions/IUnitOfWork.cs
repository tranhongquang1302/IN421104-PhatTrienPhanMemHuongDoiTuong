namespace EduHub.Domain.Abstractions;

// Unit of Work: gom nhiều thay đổi thành một lần lưu duy nhất (một transaction).
// Repository chỉ thêm/sửa/xóa trong bộ nhớ; handler gọi SaveChangesAsync đúng một lần.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
