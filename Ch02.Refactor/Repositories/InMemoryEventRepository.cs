using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public sealed class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = new();

    public void Add(Event ev) => _events.Add(ev);

    public IReadOnlyList<Event> GetByClub(Guid clubId) =>
        _events.Where(ev => ev.ClubId == clubId).ToArray();
}
