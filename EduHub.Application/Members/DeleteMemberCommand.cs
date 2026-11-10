using EduHub.Domain.Interfaces;

namespace EduHub.Application.Members;

public record DeleteMemberCommand(Guid Id);

public class DeleteMemberCommandHandler
{
    private readonly IMemberRepository _repository;

    public DeleteMemberCommandHandler(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(DeleteMemberCommand command, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(command.Id, ct);
    }
}
