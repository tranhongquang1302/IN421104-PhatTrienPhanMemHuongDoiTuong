namespace EduHub.Application.Reports;

// Một dòng báo cáo: mỗi sự kiện có bao nhiêu lượt đăng ký còn hiệu lực
public class EventReportRow
{
    public string EventName { get; set; } = string.Empty;
    public string ClubName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Registered { get; set; }
    public double FillRate => Capacity == 0 ? 0 : Math.Round(Registered * 100.0 / Capacity, 1);
}

// Phía đọc (query side): báo cáo cần JOIN + GROUP BY nên viết SQL thuần bằng Dapper
public interface IEventReportQuery
{
    Task<IReadOnlyList<EventReportRow>> GetRegistrationSummaryAsync(int minRegistered, CancellationToken ct = default);
}
