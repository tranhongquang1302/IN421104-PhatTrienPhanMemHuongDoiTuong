namespace EduHub.Domain.Entities;

public class Club
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    private Club() { }

    public static Club Create(string name, string description = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Club { Name = name.Trim(), Description = description };
    }
}
