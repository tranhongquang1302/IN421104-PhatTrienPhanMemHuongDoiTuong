using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;

namespace EduHub.Application.Members;

public record CreateMemberCommand(string StudentId, string Name, string Email);

public class CreateMemberCommandHandler
{
    private readonly IMemberRepository _repository;

    public CreateMemberCommandHandler(IMemberRepository repository)
    {
        _repository = repository;
    }

    public async Task<Member> HandleAsync(CreateMemberCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ArgumentException("Tên thành viên không được để trống.");

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("Email thành viên không được để trống.");

        var member = Member.Create(command.StudentId?.Trim() ?? string.Empty, command.Name.Trim(), command.Email.Trim());
        await _repository.AddAsync(member, ct);
        return member;
    }
}
