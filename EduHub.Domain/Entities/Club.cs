namespace EduHub.Domain.Entities;

public class Club
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Club() { }

    public static Club Create(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Club { Name = name, Description = description };
    }

    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Description = description ?? string.Empty;
    }
}
