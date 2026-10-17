using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public interface IMemberRepository
{
    void Add(Member member);
    IReadOnlyList<Member> GetByClub(Guid clubId);
}
