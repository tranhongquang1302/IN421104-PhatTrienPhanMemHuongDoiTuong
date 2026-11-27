using EduHub.API.Contracts;
using EduHub.Application.Clubs;
using EduHub.Application.Common;
using EduHub.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClubsController(ClubService service) : ControllerBase
{
    // GET /api/clubs?search=keyword&page=1&pageSize=10 -> 200 { items, totalCount, page, pageSize }
    [HttpGet]
    public async Task<ActionResult<PagedResult<ClubDto>>> Search(
        [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
        => Ok(await service.SearchAsync(new SearchClubsQuery(search, page, pageSize), ct));

    // GET /api/clubs/{id} -> 200 / 404
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    // POST /api/clubs -> 201 / 400
    [HttpPost]
    public async Task<ActionResult<ClubDto>> Create(SaveClubRequest req, CancellationToken ct)
    {
        var dto = await service.CreateAsync(new SaveClubCommand(req.Name, req.Description), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // PUT /api/clubs/{id} -> 200 / 400 / 404
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClubDto>> Update(Guid id, SaveClubRequest req, CancellationToken ct)
        => Ok(await service.UpdateAsync(id, new SaveClubCommand(req.Name, req.Description), ct));

    // DELETE /api/clubs/{id} -> 204 / 404
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
