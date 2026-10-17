using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

public sealed class EventService : IEventService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly IEventRepository _events;
    private readonly IAppLogger _logger;
    private readonly INotificationService _notifier;

    public EventService(
        IClubRepository clubs,
        IMemberRepository members,
        IEventRepository events,
        IAppLogger logger,
        INotificationService notifier)
    {
        _clubs = clubs;
        _members = members;
        _events = events;
        _logger = logger;
        _notifier = notifier;
    }

    public Event CreateEvent(Guid clubId, string title, DateTime date)
    {
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            ClubId = clubId,
            Title = title,
            Date = date
        };

        _events.Add(ev);
        _logger.Info($"Event created: {title}");

        // TODO (Nhiệm vụ 3): chuyển các phản ứng sau tạo sự kiện sang Observer.
        foreach (var member in _members.GetByClub(clubId))
            _notifier.Send(member.Email, $"New event '{title}' at {club.Name}");

        return ev;
    }
}
