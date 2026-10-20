using EduHub.Domain.Enums;

namespace EduHub.Domain.Entities;

public sealed class Registration
{
    public Guid Id { get; private set; }
    public Guid MemberId { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public RegistrationStatus Status { get; private set; }

    private Registration(Guid memberId, Guid eventId)
    {
        Id = Guid.NewGuid();
        MemberId = memberId;
        EventId = eventId;
        RegisteredAt = DateTime.UtcNow;
        Status = RegistrationStatus.Pending;
    }

    // activeRegistrationCount excludes cancelled registrations.
    public static Registration Create(Member member, Event targetEvent, int activeRegistrationCount)
    {
        ArgumentNullException.ThrowIfNull(member);
        ArgumentNullException.ThrowIfNull(targetEvent);

        if (!member.IsActive())
            throw new InvalidOperationException("Inactive members cannot register.");
        if (!targetEvent.HasCapacity(activeRegistrationCount))
            throw new InvalidOperationException("Event has no remaining capacity.");

        return new Registration(member.Id, targetEvent.Id);
    }

    public void Confirm()
    {
        if (Status != RegistrationStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm a {Status} registration.");

        Status = RegistrationStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status != RegistrationStatus.Pending &&
            Status != RegistrationStatus.Confirmed)
            throw new InvalidOperationException($"Cannot cancel a {Status} registration.");

        Status = RegistrationStatus.Cancelled;
    }
}
