using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Members;

public record GetMembersQuery;

public class GetMembersQueryHandler
{
    private readonly IMemberRepository _repository;

    public GetMembersQueryHandler(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Member>> HandleAsync(GetMembersQuery query, CancellationToken ct = default)
    {
        return await _repository.GetAllAsync(ct);
    }
}
