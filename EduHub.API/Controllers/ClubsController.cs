using EduHub.API.Security;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
// TODO 3.6: thêm [Authorize] ở đây (phải đăng nhập mới được gọi mọi action; thiếu token -> 401)
public class ClubsController(EduHubDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await db.Clubs.Select(c => new { c.Id, c.Name, c.Description }).ToListAsync(ct));

    [HttpPost]
    // TODO 3.7: chỉ Admin được tạo CLB: [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Create([FromBody] CreateClubRequest req, CancellationToken ct)
    {
        var club = Club.Create(req.Name, req.Description ?? "");
        db.Clubs.Add(club);
        await db.SaveChangesAsync(ct);
        return Created($"/api/clubs/{club.Id}", new { club.Id, club.Name });
    }

    [HttpDelete("{id:guid}")]
    // TODO 3.8: chỉ Admin được xoá CLB (Member -> 403): [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var club = await db.Clubs.FindAsync([id], ct);
        if (club is null) return NotFound();
        db.Clubs.Remove(club);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}

public record CreateClubRequest(string Name, string? Description);
