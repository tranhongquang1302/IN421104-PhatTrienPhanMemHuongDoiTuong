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

    public Task<EventDto> CreateAsync(CreateEventCommand cmd, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 3] 1. Kiểm tra club tồn tại (clubs.ExistsAsync), nếu không: throw new BusinessRuleException(...)
        //  2. Event.Create(...) với StartTime đổi sang UTC, events.Add(ev)
        //  3. await uow.SaveChangesAsync(ct)   (đúng MỘT lần)
        //  4. [Nhiệm vụ 6] logger.LogInformation với thuộc tính có cấu trúc {EventId}, {ClubId}, {Capacity}
        //  5. return ev.ToDto()
        throw new NotImplementedException();
    }

    public Task<EventDto> UpdateAsync(Guid id, UpdateEventCommand cmd, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 3] Lấy event (không có thì NotFoundException); nếu cmd.Capacity nhỏ hơn số người đã đăng ký
        //  (registrations.CountActiveByEventIdAsync) thì BusinessRuleException; ev.Update(...); SaveChangesAsync; trả DTO.
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ev = await events.GetByIdAsync(id, ct) ?? throw new NotFoundException("Sự kiện không tồn tại.");
        events.Remove(ev);
        await uow.SaveChangesAsync(ct);
        logger.LogInformation("Đã xóa sự kiện {EventId}", id);
    }
}
