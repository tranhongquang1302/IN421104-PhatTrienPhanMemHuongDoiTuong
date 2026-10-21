using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public sealed class InMemoryMemberRepository : IMemberRepository
{
    private readonly List<Member> _members = new();

    public void Add(Member member) => _members.Add(member);

    public IReadOnlyList<Member> GetByClub(Guid clubId) =>
        _members.Where(member => member.ClubId == clubId).ToArray();
}
