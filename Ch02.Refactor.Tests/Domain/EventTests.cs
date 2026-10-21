using EduHub.Ch02.Domain;

namespace Ch02.Refactor.Tests.Domain;

public class EventTests
{
    [Fact]
    public void Register_WithNewMember_ReturnsTrueAndStoresRegistration()
    {
        // Arrange
        var ev = CreateEvent();
        var memberId = Guid.NewGuid();

        // Act
        var registered = ev.Register(memberId);

        // Assert
        Assert.True(registered);
        Assert.Equal(memberId, Assert.Single(ev.RegisteredMemberIds));
    }

    [Fact]
    public void Register_WithSameMemberTwice_ReturnsFalseAndKeepsOneRegistration()
    {
        // Arrange
        var ev = CreateEvent();
        var memberId = Guid.NewGuid();
        ev.Register(memberId);

        // Act
        var registeredAgain = ev.Register(memberId);

        // Assert
        Assert.False(registeredAgain);
        Assert.Equal(memberId, Assert.Single(ev.RegisteredMemberIds));
    }

    [Fact]
    public void Register_WithDifferentMembers_KeepsBothRegistrations()
    {
        // Arrange
        var ev = CreateEvent();
        var firstMember = Guid.NewGuid();
        var secondMember = Guid.NewGuid();
        ev.Register(firstMember);

        // Act
        ev.Register(secondMember);

        // Assert
        Assert.Equal(2, ev.RegisteredMemberIds.Count);
        Assert.Contains(firstMember, ev.RegisteredMemberIds);
        Assert.Contains(secondMember, ev.RegisteredMemberIds);
    }

    [Fact]
    public void CheckIn_WithUnregisteredMember_ThrowsWithoutChangingAttendance()
    {
        // Arrange
        var ev = CreateEvent();
        var memberId = Guid.NewGuid();

        // Act
        var act = () => ev.CheckIn(memberId);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Empty(ev.RegisteredMemberIds);
        Assert.Empty(ev.CheckedInMemberIds);
    }

    [Fact]
    public void CheckIn_WithRegisteredMember_RecordsAttendance()
    {
        // Arrange
        var ev = CreateEvent();
        var memberId = Guid.NewGuid();
        ev.Register(memberId);

        // Act
        ev.CheckIn(memberId);

        // Assert
        Assert.Equal(memberId, Assert.Single(ev.CheckedInMemberIds));
    }

    [Fact]
    public void CheckIn_WithOtherMemberRegistered_StillRejectsUnknownMember()
    {
        // Arrange
        var ev = CreateEvent();
        var registeredMember = Guid.NewGuid();
        ev.Register(registeredMember);
        var unknownMember = Guid.NewGuid();

        // Act
        var act = () => ev.CheckIn(unknownMember);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Equal(registeredMember, Assert.Single(ev.RegisteredMemberIds));
        Assert.Empty(ev.CheckedInMemberIds);
    }

    [Fact]
    public void CheckIn_WithSameMemberTwice_KeepsOneAttendanceRecord()
    {
        // Arrange
        var ev = CreateEvent();
        var memberId = Guid.NewGuid();
        ev.Register(memberId);
        ev.CheckIn(memberId);

        // Act
        ev.CheckIn(memberId);

        // Assert
        Assert.Equal(memberId, Assert.Single(ev.CheckedInMemberIds));
    }

    [Fact]
    public void CheckIn_WithRegistrationForDifferentEvent_Throws()
    {
        // Arrange
        var firstEvent = CreateEvent();
        var secondEvent = CreateEvent();
        var memberId = Guid.NewGuid();
        firstEvent.Register(memberId);

        // Act
        var act = () => secondEvent.CheckIn(memberId);

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Empty(secondEvent.CheckedInMemberIds);
    }

    private static Event CreateEvent() => new()
    {
        Id = Guid.NewGuid(),
        ClubId = Guid.NewGuid(),
        Title = "Workshop SOLID",
        Date = new DateTime(2026, 11, 20)
    };
}
