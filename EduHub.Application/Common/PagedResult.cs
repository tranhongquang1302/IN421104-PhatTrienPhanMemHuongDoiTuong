namespace EduHub.Application.Common;

// Kết quả phân trang: items + tổng số bản ghi (trước khi skip/take)
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// Chuẩn hóa tham số phân trang một chỗ: page >= 1, pageSize trong [1, 100]
public readonly record struct PageRequest(int Page, int PageSize)
{
    public const int MaxPageSize = 100;

    public static PageRequest Of(int page, int pageSize)
        => new(Math.Max(page, 1), Math.Clamp(pageSize, 1, MaxPageSize));

    public int Skip => (Page - 1) * PageSize;
}
