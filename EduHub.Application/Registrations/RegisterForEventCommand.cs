using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Registrations;

public record RegisterForEventCommand(Guid MemberId, Guid EventId);

public class RegisterForEventCommandHandler(IEventRepository events, IRegistrationRepository registrations,
    IUnitOfWork uow, ILogger<RegisterForEventCommandHandler> logger)
{
    public async Task<RegistrationDto> HandleAsync(RegisterForEventCommand command, CancellationToken ct = default)
    {
        await using var transaction = await uow.BeginTransactionAsync(ct);
        var ev = await events.GetForUpdateAsync(command.EventId, ct)
            ?? throw new NotFoundException("Sự kiện không tồn tại.");
        if (!await registrations.MemberExistsAsync(command.MemberId, ct))
            throw new NotFoundException("Thành viên không tồn tại.");
        if (await registrations.ExistsAsync(command.MemberId, ev.Id, ct))
            throw new ConflictException("Thành viên đã đăng ký sự kiện này.");

        var registered = await registrations.CountActiveByEventIdAsync(ev.Id, ct);
        if (ev.IsFull(registered))
        {
            logger.LogWarning("Sự kiện {EventId} đã đạt sức chứa {Capacity}", ev.Id, ev.Capacity);
            throw new BusinessRuleException("Sự kiện đã đạt giới hạn.");
        }

        var registration = Registration.Create(command.MemberId, ev.Id);
        registrations.Add(registration);
        await uow.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        logger.LogInformation("Đã tạo đăng ký {RegistrationId} cho sự kiện {EventId} và thành viên {MemberId}",
            registration.Id, ev.Id, registration.MemberId);
        return registration.ToDto();
    }
}
