using EduHub.Ch02.Domain;
using EduHub.Ch02.Patterns.Observers;

namespace EduHub.Ch02.Services;

public interface IEventService
{
    void Subscribe(IEventObserver observer);
    bool Unsubscribe(IEventObserver observer);
    Event CreateEvent(Guid clubId, string title, DateTime date);
}
