namespace EduHub.Ch02.Domain;

/// <summary>Câu lạc bộ, quản lý quy tắc tên và giới hạn số thành viên.</summary>
public class Club
{
    public const int MaxMembers = 50;
    public const int MaxNameLength = 100;

    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }

    private Club(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }

    public static Club Create(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Club name is required");
        if (name.Length > MaxNameLength)
            throw new ArgumentException("Club name too long");

        return new Club(name, description);
    }

    public bool HasRoom(int currentMemberCount) => currentMemberCount < MaxMembers;
}
