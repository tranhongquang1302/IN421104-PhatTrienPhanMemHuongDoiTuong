using System.ComponentModel.DataAnnotations;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Clubs;

public record CreateClubCommand(string Name, string? Description);

public class CreateClubCommandHandler(IClubRepository repo)
{
    public async Task<Club> HandleAsync(CreateClubCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            throw new ValidationException("Tên CLB không được để trống.");

        var club = Club.Create(cmd.Name.Trim(), cmd.Description ?? "");
        await repo.AddAsync(club, ct);
        return club;
    }
}
