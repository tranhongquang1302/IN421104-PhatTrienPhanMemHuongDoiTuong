using EduHub.Ch02.Logging;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

public sealed class ReportService : IReportService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly IEventRepository _events;
    private readonly IAppLogger _logger;

    public ReportService(
        IClubRepository clubs,
        IMemberRepository members,
        IEventRepository events,
        IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _events = events;
        _logger = logger;
    }

    public string Generate(IReportStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        var stats = new List<ClubStat>();
        foreach (var club in _clubs.GetAll())
        {
            var memberCount = _members.GetByClub(club.Id).Count;
            var events = _events.GetByClub(club.Id);
            stats.Add(new ClubStat(
                club.Name,
                memberCount,
                events.Count,
                events.Sum(ev => ev.RegisteredMemberIds.Count),
                events.Sum(ev => ev.CheckedInMemberIds.Count)));
            _logger.Info($"Report generated for {club.Name}");
        }

        return strategy.Render(stats);
    }
}
