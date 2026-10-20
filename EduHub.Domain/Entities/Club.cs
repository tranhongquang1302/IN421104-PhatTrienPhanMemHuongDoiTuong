namespace EduHub.Domain.Entities;

public sealed class Club
{
    private readonly List<Member> _members = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyList<Member> Members => _members.AsReadOnly();

    private Club(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }

    public static Club Create(string name, string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name is required.", nameof(name));

        return new Club(name.Trim(), (description ?? "").Trim());
    }

    public void AddMember(Member member)
    {
        ArgumentNullException.ThrowIfNull(member);
        if (_members.Any(m => m.Id == member.Id))
            throw new InvalidOperationException("Member already belongs to this club.");
        _members.Add(member);
    }

    public int MemberCount() => _members.Count;

    // Added for the student's US-03: change the club description.
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Club description cannot be empty.", nameof(description));

        Description = description.Trim();
    }
}
