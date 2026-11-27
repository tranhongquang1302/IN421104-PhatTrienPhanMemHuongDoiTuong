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
    public Task<RegistrationDto> HandleAsync(RegisterForEventCommand command, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 4] Kiểm tra theo thứ tự, mỗi lỗi ném đúng loại exception:
        //  1. Sự kiện không tồn tại      -> NotFoundException   (404)
        //  2. Thành viên không tồn tại   -> NotFoundException   (404)
        //  3. Đã đăng ký sự kiện này     -> ConflictException   (409)
        //  4. Sự kiện đầy (ev.IsFull)    -> BusinessRuleException("Sự kiện đã đạt giới hạn.") (400) + logger.LogWarning
        //  Hợp lệ: Registration.Create, registrations.Add, uow.SaveChangesAsync (một lần), LogInformation, trả DTO.
        throw new NotImplementedException();
    }
}
