namespace EduHub.Domain.Entities;

public class Registration
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Guid EventId { get; set; }
    public DateTime RegisteredAt { get; private set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

    private Registration() { }

    public static Registration Create(Guid memberId, Guid eventId) =>
        new() { MemberId = memberId, EventId = eventId };

    public void Cancel() => Status = "Cancelled";
    public void Confirm() => Status = "Confirmed";
}
