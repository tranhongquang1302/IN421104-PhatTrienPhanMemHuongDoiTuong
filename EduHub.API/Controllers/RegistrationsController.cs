using EduHub.API.Contracts;
using EduHub.Application.Dtos;
using EduHub.Application.Registrations;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/registrations")]
public class RegistrationsController(
    RegisterForEventCommandHandler registerHandler,
    GetEventRegistrationsQueryHandler listHandler,
    CancelRegistrationCommandHandler cancelHandler) : ControllerBase
{
    // POST -> 201 / 400 (đầy) / 404 (không có event/member) / 409 (đăng ký rồi)
    [HttpPost]
    public async Task<ActionResult<RegistrationDto>> Register(Guid eventId, RegisterRequest req, CancellationToken ct)
    {
        var dto = await registerHandler.HandleAsync(new RegisterForEventCommand(req.MemberId, eventId), ct);
        return CreatedAtAction(nameof(GetByEvent), new { eventId }, dto);
    }

    // GET -> 200 danh sách member đã đăng ký (kèm thông tin cơ bản) / 404
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RegistrationMemberDto>>> GetByEvent(Guid eventId, CancellationToken ct)
        => Ok(await listHandler.HandleAsync(eventId, ct));

    // DELETE /{registrationId} -> 204 / 404 (hủy đăng ký)
    [HttpDelete("{registrationId:guid}")]
    public async Task<IActionResult> Cancel(Guid eventId, Guid registrationId, CancellationToken ct)
    {
        await cancelHandler.HandleAsync(eventId, registrationId, ct);
        return NoContent();
    }
}
