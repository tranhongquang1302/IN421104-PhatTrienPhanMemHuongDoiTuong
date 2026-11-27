using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Clubs;

public record GetClubsQuery;

public class GetClubsQueryHandler(IClubRepository repo)
{
    public Task<IEnumerable<Club>> HandleAsync(GetClubsQuery query, CancellationToken ct = default)
        => repo.GetAllAsync(ct);
}
