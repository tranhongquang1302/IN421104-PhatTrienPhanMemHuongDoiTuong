using EduHub.API.Security;
using EduHub.Application.CheckIns;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController(EduHubDbContext db, CheckInService checkIns) : ControllerBase
{
    private Guid CurrentMemberId => Guid.Parse(User.FindFirst("sub")!.Value);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.Events.Select(e => new { e.Id, e.Name, e.ClubId, e.StartTime, e.Capacity }).ToListAsync(ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var ev = await db.Events.FindAsync([id], ct);
        if (ev is null) return NotFound();
        db.Events.Remove(ev);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    /// <summary>Member (hoặc Admin) đăng ký sự kiện cho chính mình.</summary>
    [HttpPost("{id:guid}/register")]
    public async Task<IActionResult> Register(Guid id, CancellationToken ct)
    {
        if (!await db.Events.AnyAsync(e => e.Id == id, ct)) return NotFound();
        var memberId = CurrentMemberId;
        if (await db.Registrations.AnyAsync(r => r.MemberId == memberId && r.EventId == id, ct))
            return Conflict(new { error = "Đã đăng ký sự kiện này rồi." });

        var reg = Registration.Create(memberId, id);
        reg.Confirm();
        db.Registrations.Add(reg);
        await db.SaveChangesAsync(ct);
        return Created($"/api/events/{id}", new { registrationId = reg.Id, reg.Status });
    }

    /// <summary>Admin (hoặc người có claim qr.issue) tạo QR hạn 15 phút cho sự kiện.</summary>
    [HttpPost("{id:guid}/generate-qr")]
    [Authorize(Policy = Policies.CanIssueQr)]
    public async Task<IActionResult> GenerateQr(Guid id, CancellationToken ct)
    {
        if (!await db.Events.AnyAsync(e => e.Id == id, ct)) return NotFound();
        var qr = checkIns.GenerateQr(id);
        return Ok(new { qrPayload = qr.Payload, expiresAt = qr.ExpiresAt });
    }

    /// <summary>Member quét QR để điểm danh.</summary>
    [HttpPost("{id:guid}/checkin")]
    [EnableRateLimiting("checkin")]   // chống thử QR hàng loạt
    public async Task<IActionResult> CheckIn(Guid id, [FromBody] CheckInRequest req, CancellationToken ct)
    {
        var result = await checkIns.CheckInAsync(CurrentMemberId, id, req.QrPayload, ct);
        return result.Outcome switch
        {
            CheckInOutcome.Success => Ok(new { message = "Check-in thành công", checkedInAt = result.CheckedInAt }),
            CheckInOutcome.EventNotFound => NotFound(new { error = "Không tìm thấy sự kiện." }),
            CheckInOutcome.InvalidQr => BadRequest(new { error = "QR không hợp lệ." }),
            CheckInOutcome.ExpiredQr => BadRequest(new { error = "QR đã hết hạn (quá 15 phút)." }),
            CheckInOutcome.NotRegistered => StatusCode(StatusCodes.Status403Forbidden, new { error = "Bạn chưa đăng ký sự kiện này." }),
            CheckInOutcome.AlreadyCheckedIn => Conflict(new { error = "Bạn đã check-in rồi." }),
            _ => StatusCode(500)
        };
    }
}

public record CheckInRequest(string QrPayload);
