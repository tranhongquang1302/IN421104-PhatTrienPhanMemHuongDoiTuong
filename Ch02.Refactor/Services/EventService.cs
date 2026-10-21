using EduHub.Ch02.Domain;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

public sealed class EventService : IEventService
{
    private readonly IClubRepository _clubs;
    private readonly IEventRepository _events;
    private readonly List<IEventObserver> _observers = new();

    public EventService(IClubRepository clubs, IEventRepository events)
    {
        _clubs = clubs;
        _events = events;
    }

    public void Subscribe(IEventObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        if (!_observers.Contains(observer))
            _observers.Add(observer);
    }

    public bool Unsubscribe(IEventObserver observer) => _observers.Remove(observer);

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

        // Dùng snapshot để thay đổi đăng ký trong callback có hiệu lực từ sự kiện sau.
        foreach (var observer in _observers.ToArray())
            observer.OnEventCreated(ev, club);

        return ev;
    }
}
