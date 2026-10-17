using EduHub.Ch02.Logging;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

public sealed class ReportService : IReportService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly IAppLogger _logger;

    public ReportService(IClubRepository clubs, IMemberRepository members, IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _logger = logger;
    }

    // TODO (Nhiệm vụ 3): dùng IReportStrategy để hỗ trợ định dạng Console và CSV.
    public string Generate()
    {
        var lines = new List<string>();
        foreach (var club in _clubs.GetAll())
        {
            var memberCount = _members.GetByClub(club.Id).Count;
            lines.Add($"Club: {club.Name} | Members: {memberCount}");
            _logger.Info($"Report generated for {club.Name}");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
