using System.ComponentModel.DataAnnotations;
using EduHub.API.Security;
using EduHub.Application.Clubs;
using EduHub.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

public record CreateClubRequest(string Name, string? Description);

[ApiController]
[Route("api/[controller]")]
[Authorize]   // phải đăng nhập mới được gọi mọi action (thiếu token -> 401)
public class ClubsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromServices] GetClubsQueryHandler handler, CancellationToken ct)
    {
        var clubs = await handler.HandleAsync(new GetClubsQuery(), ct);
        return Ok(clubs.Select(c => new { c.Id, c.Name, c.Description }));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id, [FromServices] GetClubByIdQueryHandler handler, CancellationToken ct)
    {
        var club = await handler.HandleAsync(new GetClubByIdQuery(id), ct);
        return club is null ? NotFound() : Ok(new { club.Id, club.Name, club.Description });
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> Create(
        [FromBody] CreateClubRequest req, [FromServices] CreateClubCommandHandler handler, CancellationToken ct)
    {
        try
        {
            var club = await handler.HandleAsync(new CreateClubCommand(req.Name, req.Description), ct);
            return CreatedAtAction(nameof(GetById), new { id = club.Id }, new { club.Id, club.Name, club.Description });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Policies.AdminOnly)]   // Member -> 403
    public async Task<IActionResult> Delete(Guid id, [FromServices] IClubRepository clubs, CancellationToken ct)
        => await clubs.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
