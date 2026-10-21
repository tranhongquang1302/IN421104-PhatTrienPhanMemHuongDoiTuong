namespace EduHub.Ch02.Domain;

/// <summary>Sự kiện của CLB, kèm danh sách đăng ký và check-in.</summary>
public class Event
{
    private readonly HashSet<Guid> _registeredMemberIds = new();
    private readonly HashSet<Guid> _checkedInMemberIds = new();

    public Guid Id { get; set; }
    public Guid ClubId { get; set; }
    public required string Title { get; set; }
    public DateTime Date { get; set; }

    public IReadOnlyCollection<Guid> RegisteredMemberIds => _registeredMemberIds.ToArray();
    public IReadOnlyCollection<Guid> CheckedInMemberIds => _checkedInMemberIds.ToArray();

    public bool Register(Guid memberId) => _registeredMemberIds.Add(memberId);

    public void CheckIn(Guid memberId)
    {
        if (!_registeredMemberIds.Contains(memberId))
            throw new InvalidOperationException("Member must register before check-in");

        _checkedInMemberIds.Add(memberId);
    }
}
