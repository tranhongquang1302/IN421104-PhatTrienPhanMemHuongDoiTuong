using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Members;

public record GetMemberByIdQuery(Guid Id);

public class GetMemberByIdQueryHandler
{
    private readonly IMemberRepository _repository;

    public GetMemberByIdQueryHandler(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<Member?> HandleAsync(GetMemberByIdQuery query, CancellationToken ct = default)
    {
        return await _repository.GetByIdAsync(query.Id, ct);
    }
}
