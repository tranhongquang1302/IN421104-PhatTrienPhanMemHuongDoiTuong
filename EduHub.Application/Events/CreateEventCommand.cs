using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Events;

public record CreateEventCommand(Guid ClubId, string Name, int Capacity, DateTime StartTime);

public class CreateEventCommandHandler(IEventRepository events, IClubRepository clubs)
{
    public async Task<Event> HandleAsync(CreateEventCommand cmd, CancellationToken ct = default)
    {
        _ = await clubs.GetByIdAsync(cmd.ClubId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy CLB.");

        var ev = Event.Create(cmd.Name, cmd.ClubId, cmd.StartTime, cmd.Capacity);
        await events.AddAsync(ev, ct);
        return ev;
    }
}
