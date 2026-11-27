namespace EduHub.Domain.Entities;

public class Member
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string StudentId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private Member() { }

    public static Member Create(string studentId, string fullName, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(studentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return new Member { StudentId = studentId, FullName = fullName, Email = email };
    }
}
