using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;

namespace EduHub.Application.Clubs;

public record SearchClubsQuery(string? Search, int Page = 1, int PageSize = 10);
public record SaveClubCommand(string Name, string? Description);

public class ClubService(IClubRepository clubs, IUnitOfWork uow)
{
    public async Task<PagedResult<ClubDto>> SearchAsync(SearchClubsQuery q, CancellationToken ct = default)
    {
        var page = PageRequest.Of(q.Page, q.PageSize);
        var (clubsOnPage, totalCount) = await clubs.SearchAsync(q.Search, page.Skip, page.PageSize, ct);
        return new PagedResult<ClubDto>(clubsOnPage.Select(club => club.ToDto()).ToList(),
            totalCount, page.Page, page.PageSize);
    }

    public async Task<ClubDto> GetByIdAsync(Guid id, CancellationToken ct = default)
        => (await clubs.GetByIdAsync(id, ct) ?? throw new NotFoundException("Câu lạc bộ không tồn tại.")).ToDto();

    public async Task<ClubDto> CreateAsync(SaveClubCommand cmd, CancellationToken ct = default)
    {
        var club = Club.Create(cmd.Name, cmd.Description ?? string.Empty);
        clubs.Add(club);
        await uow.SaveChangesAsync(ct);
        return club.ToDto();
    }

    public async Task<ClubDto> UpdateAsync(Guid id, SaveClubCommand cmd, CancellationToken ct = default)
    {
        var club = await clubs.GetByIdAsync(id, ct) ?? throw new NotFoundException("Câu lạc bộ không tồn tại.");
        club.Update(cmd.Name, cmd.Description);
        await uow.SaveChangesAsync(ct);
        return club.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var club = await clubs.GetByIdAsync(id, ct) ?? throw new NotFoundException("Câu lạc bộ không tồn tại.");
        clubs.Remove(club);
        await uow.SaveChangesAsync(ct);
    }
}
