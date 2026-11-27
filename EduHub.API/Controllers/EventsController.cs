using EduHub.API.Contracts;
using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Application.Events;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(GetEventsQueryHandler listHandler, EventService service) : ControllerBase
{
    // GET /api/events?clubId=&search=&page=1&pageSize=10 -> 200 (PagedResult)
    [HttpGet]
    public async Task<ActionResult<PagedResult<EventDto>>> GetAll(
        [FromQuery] Guid? clubId, [FromQuery] string? search,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        => Ok(await listHandler.HandleAsync(new GetEventsQuery(clubId, search, page, pageSize), ct));

    // GET /api/events/{id} -> 200 / 404
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(id, ct));

    // POST /api/events -> 201 / 400
    [HttpPost]
    public async Task<ActionResult<EventDto>> Create(CreateEventRequest req, CancellationToken ct)
    {
        var dto = await service.CreateAsync(
            new CreateEventCommand(req.Name, req.Description, req.ClubId, req.Location, req.StartTime, req.Capacity), ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // PUT /api/events/{id} -> 200 / 400 / 404
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EventDto>> Update(Guid id, UpdateEventRequest req, CancellationToken ct)
        => Ok(await service.UpdateAsync(id,
            new UpdateEventCommand(req.Name, req.Description, req.Location, req.StartTime, req.Capacity), ct));

    // DELETE /api/events/{id} -> 204 / 404
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
