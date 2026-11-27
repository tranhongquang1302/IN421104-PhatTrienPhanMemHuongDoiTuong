using EduHub.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IEventReportQuery report) : ControllerBase
{
    // GET /api/reports/events?minRegistered=0 -> báo cáo số lượt đăng ký mỗi sự kiện (Dapper, SQL thuần)
    [HttpGet("events")]
    public async Task<ActionResult<IReadOnlyList<EventReportRow>>> Events(
        [FromQuery] int minRegistered = 0, CancellationToken ct = default)
        => Ok(await report.GetRegistrationSummaryAsync(Math.Max(minRegistered, 0), ct));
}
