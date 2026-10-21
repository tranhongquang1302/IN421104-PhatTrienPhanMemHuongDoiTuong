using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

public sealed class ClubService : IClubService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly IAppLogger _logger;
    private readonly INotificationService _clubNotifier;
    private readonly IReadOnlyList<INotificationService> _memberNotifiers;

    public ClubService(
        IClubRepository clubs,
        IMemberRepository members,
        IAppLogger logger,
        INotificationService clubNotifier,
        IEnumerable<INotificationService> memberNotifiers)
    {
        _clubs = clubs;
        _members = members;
        _logger = logger;
        _clubNotifier = clubNotifier;
        _memberNotifiers = memberNotifiers.ToArray();
    }

    public Club CreateClub(string name, string description, string presidentEmail)
    {
        var club = Club.Create(name, description);
        _clubs.Add(club);
        _logger.Info($"Club created: {club.Name}");
        _clubNotifier.Send(presidentEmail, $"Club created: {club.Name}");
        return club;
    }

    public Member AddMemberToClub(Guid clubId, string memberName, string email)
    {
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        if (!club.HasRoom(_members.GetByClub(clubId).Count))
            throw new InvalidOperationException($"Club is full (max {Club.MaxMembers} members)");

        var member = new Member
        {
            Id = Guid.NewGuid(),
            Name = memberName,
            Email = email,
            ClubId = clubId
        };

        _members.Add(member);
        _logger.Info($"Member {memberName} joined {club.Name}");
        foreach (var notifier in _memberNotifiers)
            notifier.Send(email, $"Welcome! {memberName} joined {club.Name}");

        return member;
    }

    public IReadOnlyList<Club> GetAllClubs() => _clubs.GetAll();
}
