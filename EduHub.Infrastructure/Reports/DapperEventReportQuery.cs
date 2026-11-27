using Dapper;
using EduHub.Application.Reports;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Reports;

// Dapper: SQL tự viết, tham số hóa nên không bị SQL injection. Dùng chung kết nối của DbContext.
public class DapperEventReportQuery(EduHubDbContext db) : IEventReportQuery
{
    private const string Sql = """
        SELECT e."Name" AS "EventName",
               c."Name" AS "ClubName",
               e."Capacity" AS "Capacity",
               CAST(COUNT(r."Id") AS integer) AS "Registered"
        FROM events AS e
        INNER JOIN clubs AS c ON c."Id" = e."ClubId"
        LEFT JOIN registrations AS r ON r."EventId" = e."Id" AND r."Status" <> @cancelledStatus
        GROUP BY e."Id", e."Name", c."Name", e."Capacity"
        HAVING COUNT(r."Id") >= @minRegistered
        ORDER BY "Registered" DESC, e."Name", e."Id"
        """;

    public async Task<IReadOnlyList<EventReportRow>> GetRegistrationSummaryAsync(
        int minRegistered, CancellationToken ct = default)
    {
        var command = new CommandDefinition(Sql, new { minRegistered, cancelledStatus = Registration.CancelledStatus },
            cancellationToken: ct);
        var rows = await db.Database.GetDbConnection().QueryAsync<EventReportRow>(command);
        return rows.AsList();
    }
}
