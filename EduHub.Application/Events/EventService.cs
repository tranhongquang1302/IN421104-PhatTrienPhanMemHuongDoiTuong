using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Events;

public record CreateEventCommand(string Name, string? Description, Guid ClubId, string? Location, DateTime StartTime, int Capacity);
public record UpdateEventCommand(string Name, string? Description, string? Location, DateTime StartTime, int Capacity);

public class EventService(IEventRepository events, IClubRepository clubs, IRegistrationRepository registrations,
    IUnitOfWork uow, ILogger<EventService> logger)
{
    public async Task<EventDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await events.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sự kiện không tồn tại.")).ToDto();

    public async Task<EventDto> CreateAsync(CreateEventCommand cmd, CancellationToken ct = default)
    {
        if (!await clubs.ExistsAsync(cmd.ClubId, ct))
            throw new BusinessRuleException("Câu lạc bộ không tồn tại.");

        var ev = Event.Create(cmd.Name, cmd.ClubId, cmd.StartTime.ToUniversalTime(), cmd.Capacity,
            cmd.Description, cmd.Location);
        events.Add(ev);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Đã tạo sự kiện {EventId} của câu lạc bộ {ClubId} với sức chứa {Capacity}",
            ev.Id, ev.ClubId, ev.Capacity);
        return ev.ToDto();
    }

    public async Task<EventDto> UpdateAsync(Guid id, UpdateEventCommand cmd, CancellationToken ct = default)
    {
        await using var transaction = await uow.BeginTransactionAsync(ct);
        var ev = await events.GetForUpdateAsync(id, ct) ?? throw new NotFoundException("Sự kiện không tồn tại.");
        var registered = await registrations.CountActiveByEventIdAsync(id, ct);
        if (cmd.Capacity < registered)
            throw new BusinessRuleException("Sức chứa không được nhỏ hơn số lượt đăng ký còn hiệu lực.");

        ev.Update(cmd.Name, cmd.Description, cmd.Location, cmd.StartTime.ToUniversalTime(), cmd.Capacity);
        await uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        logger.LogInformation("Đã cập nhật sự kiện {EventId} với sức chứa {Capacity}", ev.Id, ev.Capacity);
        return ev.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ev = await events.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sự kiện không tồn tại.");
        events.Remove(ev);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Đã xóa sự kiện {EventId}", id);
    }
}
