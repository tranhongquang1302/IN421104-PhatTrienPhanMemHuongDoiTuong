using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;

namespace EduHub.Application.Events;

public record GetEventsQuery(Guid? ClubId = null, string? Search = null, int Page = 1, int PageSize = 10);

public class GetEventsQueryHandler(IEventRepository events)
{
    public async Task<PagedResult<EventDto>> HandleAsync(GetEventsQuery query, CancellationToken ct = default)
    {
        var page = PageRequest.Of(query.Page, query.PageSize);
        var (eventsOnPage, totalCount) = await events.SearchAsync(
            query.ClubId, query.Search, page.Skip, page.PageSize, ct);
        return new PagedResult<EventDto>(eventsOnPage.Select(ev => ev.ToDto()).ToList(),
            totalCount, page.Page, page.PageSize);
    }
}
