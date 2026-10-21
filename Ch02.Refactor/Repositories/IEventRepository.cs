using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public interface IEventRepository
{
    void Add(Event ev);
    IReadOnlyList<Event> GetByClub(Guid clubId);
}
