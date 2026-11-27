using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using Microsoft.Extensions.Logging;

namespace EduHub.Application.Registrations;

public class GetEventRegistrationsQueryHandler(IEventRepository events, IRegistrationRepository registrations)
{
    public async Task<IReadOnlyList<RegistrationMemberDto>> HandleAsync(Guid eventId, CancellationToken ct = default)
    {
        if (await events.GetByIdAsync(eventId, ct) is null)
            throw new NotFoundException("Sự kiện không tồn tại.");
        var list = await registrations.GetByEventAsync(eventId, ct);
        return list.Select(x => x.ToDto()).ToList();
    }
}

public class CancelRegistrationCommandHandler(IRegistrationRepository registrations, IUnitOfWork uow,
    ILogger<CancelRegistrationCommandHandler> logger)
{
    public Task HandleAsync(Guid eventId, Guid registrationId, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 4] registrations.GetAsync (không có thì NotFoundException), reg.Cancel(), SaveChangesAsync, LogInformation.
        //  Hủy = đổi Status sang Cancelled, không xóa dòng.
        throw new NotImplementedException();
    }
}
