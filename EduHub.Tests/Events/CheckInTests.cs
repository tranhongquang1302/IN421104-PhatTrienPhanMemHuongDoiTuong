using EduHub.Domain.Entities;
using FluentAssertions;

namespace EduHub.Tests.Events;

public class CheckInTests
{
    private static readonly DateTime CheckedInAt = new(2026, 12, 12, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Verify_FirstTime_SavesTimestampAndMarksVerified()
    {
        var registrationId = Guid.NewGuid();
        var checkIn = CheckIn.CreateForRegistration(registrationId);

        checkIn.Verify(CheckedInAt);

        checkIn.RegistrationId.Should().Be(registrationId);
        checkIn.IsVerified.Should().BeTrue();
        checkIn.CheckedInAt.Should().Be(CheckedInAt);
    }

    [Fact]
    public void Verify_SecondTime_ThrowsAndKeepsOriginalTimestamp()
    {
        var checkIn = CheckIn.CreateForRegistration(Guid.NewGuid());
        checkIn.Verify(CheckedInAt);

        var verify = () => checkIn.Verify(CheckedInAt.AddMinutes(1));

        verify.Should().Throw<InvalidOperationException>();
        checkIn.IsVerified.Should().BeTrue();
        checkIn.CheckedInAt.Should().Be(CheckedInAt);
    }
}
