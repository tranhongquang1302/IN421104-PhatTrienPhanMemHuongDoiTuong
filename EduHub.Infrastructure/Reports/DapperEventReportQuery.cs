using Dapper;
using EduHub.Application.Reports;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Reports;

// Dapper: SQL tự viết, tham số hóa nên không bị SQL injection. Dùng chung kết nối của DbContext.
public class DapperEventReportQuery(EduHubDbContext db) : IEventReportQuery
{
    // TODO [Nhiệm vụ 5] Viết câu SQL báo cáo: mỗi sự kiện có bao nhiêu lượt đăng ký còn hiệu lực.
    //  Cột trả về (đặt alias đúng tên thuộc tính của EventReportRow): EventName, ClubName, Capacity, Registered
    //  - JOIN clubs, LEFT JOIN registrations (bỏ Status = 'Cancelled') để sự kiện chưa ai đăng ký vẫn có dòng
    //  - GROUP BY ..., HAVING COUNT(...) >= @minRegistered, ORDER BY Registered giảm dần
    //  Lưu ý PostgreSQL: tên bảng/cột viết hoa phải đặt trong dấu nháy kép, ví dụ e."Name".
    private const string Sql = "";

    public async Task<IReadOnlyList<EventReportRow>> GetRegistrationSummaryAsync(
        int minRegistered, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 5] Lấy kết nối bằng db.Database.GetDbConnection(), gọi conn.QueryAsync<EventReportRow>(...)
        //  truyền tham số new { minRegistered } và CancellationToken (CommandDefinition).
        throw new NotImplementedException();
    }
}
