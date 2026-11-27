namespace EduHub.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ClubId { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Event() { }

    public static Event Create(string name, Guid clubId, DateTime startTime, int capacity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (capacity <= 0) throw new ArgumentException("Capacity must be positive");
        return new Event { Name = name, ClubId = clubId, StartTime = startTime, Capacity = capacity };
    }

    public bool IsFull(int registrationCount) => registrationCount >= Capacity;
}
