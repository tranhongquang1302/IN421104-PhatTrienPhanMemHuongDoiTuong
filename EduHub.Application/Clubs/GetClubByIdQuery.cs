using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Clubs;

public record GetClubByIdQuery(Guid Id);

public class GetClubByIdQueryHandler(IClubRepository repo)
{
    /// <summary>Trả null nếu không có CLB (controller đổi thành 404).</summary>
    public Task<Club?> HandleAsync(GetClubByIdQuery query, CancellationToken ct = default)
        => repo.GetByIdAsync(query.Id, ct);
}
