using EduHub.Application.Common;
using EduHub.Application.Dtos;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;

namespace EduHub.Application.Clubs;

public record SearchClubsQuery(string? Search, int Page = 1, int PageSize = 10);
public record SaveClubCommand(string Name, string? Description);

public class ClubService(IClubRepository clubs, IUnitOfWork uow)
{
    public Task<PagedResult<ClubDto>> SearchAsync(SearchClubsQuery q, CancellationToken ct = default)
    {
        // TODO [Nhiệm vụ 3] Chuẩn hóa phân trang bằng PageRequest.Of(q.Page, q.PageSize),
        //  gọi clubs.SearchAsync(q.Search, paging.Skip, paging.PageSize, ct),
        //  map từng Club sang ClubDto (c.ToDto()) rồi trả PagedResult<ClubDto>.
        throw new NotImplementedException();
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
