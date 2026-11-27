using EduHub.Application.Events;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Events;

public class CreateEventCommandTests
{
    private static readonly DateTime StartTime = new(2026, 12, 12, 9, 0, 0, DateTimeKind.Utc);
    private readonly Mock<IEventRepository> events = new();
    private readonly Mock<IClubRepository> clubs = new();

    [Fact]
    public async Task CreateEvent_WithValidData_SavesAndReturnsEvent()
    {
        var club = Club.Create("CLB Robotics", "");
        clubs.Setup(r => r.GetByIdAsync(club.Id, default)).ReturnsAsync(club);
        var handler = new CreateEventCommandHandler(events.Object, clubs.Object);

        var result = await handler.HandleAsync(new CreateEventCommand(club.Id, "Workshop", 20, StartTime));

        result.Id.Should().NotBeEmpty();
        result.ClubId.Should().Be(club.Id);
        result.Name.Should().Be("Workshop");
        result.StartTime.Should().Be(StartTime);
        result.Capacity.Should().Be(20);
        events.Verify(r => r.AddAsync(result, default), Times.Once);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task CreateEvent_WithNonPositiveCapacity_ThrowsArgumentException(int capacity)
    {
        var club = Club.Create("CLB Robotics", "");
        clubs.Setup(r => r.GetByIdAsync(club.Id, default)).ReturnsAsync(club);
        var handler = new CreateEventCommandHandler(events.Object, clubs.Object);

        var create = () => handler.HandleAsync(new CreateEventCommand(club.Id, "Workshop", capacity, StartTime));

        await create.Should().ThrowAsync<ArgumentException>();
        events.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateEvent_WhenClubNotFound_ThrowsKeyNotFoundException()
    {
        var clubId = Guid.NewGuid();
        clubs.Setup(r => r.GetByIdAsync(clubId, default)).ReturnsAsync((Club?)null);
        var handler = new CreateEventCommandHandler(events.Object, clubs.Object);

        var create = () => handler.HandleAsync(new CreateEventCommand(clubId, "Workshop", 20, StartTime));

        await create.Should().ThrowAsync<KeyNotFoundException>();
        events.Verify(r => r.AddAsync(It.IsAny<Event>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
