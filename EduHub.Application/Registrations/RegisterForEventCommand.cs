using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Registrations;

/// <summary>MemberId là Id của người dùng đang đăng nhập (claim "sub" trong JWT).</summary>
public record RegisterForEventCommand(Guid EventId, Guid MemberId);

public class RegisterForEventCommandHandler(IEventRepository events, IRegistrationRepository registrations)
{
    public async Task<Registration> HandleAsync(RegisterForEventCommand cmd, CancellationToken ct = default)
    {
        var ev = await events.GetByIdAsync(cmd.EventId, ct)
            ?? throw new KeyNotFoundException("Không tìm thấy sự kiện.");

        if (await registrations.ExistsAsync(cmd.EventId, cmd.MemberId, ct))
            throw new InvalidOperationException("Thành viên đã đăng ký sự kiện này.");

        if (ev.IsFull(await registrations.CountByEventAsync(cmd.EventId, ct)))
            throw new InvalidOperationException("Sự kiện đã đủ chỗ.");

        var reg = Registration.Create(cmd.MemberId, cmd.EventId);
        reg.Confirm();
        await registrations.AddAsync(reg, ct);
        return reg;
    }
}
