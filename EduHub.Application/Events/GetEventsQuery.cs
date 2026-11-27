using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;

namespace EduHub.Application.Events;

public record GetEventsQuery(Guid? ClubId = null, string? Search = null, int Page = 1, int PageSize = 10);

public class GetEventsQueryHandler(IEventRepository events)
{
    public Task<PagedResult<EventDto>> HandleAsync(GetEventsQuery query, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 3] Giống ClubService.SearchAsync: PageRequest.Of -> events.SearchAsync -> map sang EventDto.
        throw new NotImplementedException();
    }
}
