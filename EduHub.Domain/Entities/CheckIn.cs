using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities;

public sealed class CheckIn
{
    public Guid Id { get; private set; }
    public Guid RegistrationId { get; private set; }
    public DateTime CheckedInAt { get; private set; }
    public string Method { get; private set; }

    private CheckIn(Guid registrationId, string method)
    {
        Id = Guid.NewGuid();
        RegistrationId = registrationId;
        CheckedInAt = DateTime.UtcNow;
        Method = method;
    }

    public static CheckIn Create(Registration registration, string method = "Manual")
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (registration.Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed registrations can check in.");
        if (string.IsNullOrWhiteSpace(method))
            throw new ArgumentException("Check-in method is required.", nameof(method));

        return new CheckIn(registration.Id, method.Trim());
    }

    public string Summary() =>
        $"Registration: {RegistrationId}; Method: {Method}; Time (UTC): {CheckedInAt:yyyy-MM-dd HH:mm:ss}";
}
