using EduHub.Application.Clubs;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Clubs;

public class GetClubsQueryTests
{
    [Fact]
    public async Task GetClubs_ReturnsAllClubs()
    {
        // Arrange
        var clubs = new List<Club> { Club.Create("A", "desc"), Club.Create("B", "desc2") };
        var mockRepo = new Mock<IClubRepository>();
        mockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(clubs);
        var handler = new GetClubsQueryHandler(mockRepo.Object);

        // Act
        var result = await handler.HandleAsync(new GetClubsQuery());

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetClubById_WhenNotFound_ReturnsNull()
    {
        var mockRepo = new Mock<IClubRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((Club?)null);
        var handler = new GetClubByIdQueryHandler(mockRepo.Object);

        var result = await handler.HandleAsync(new GetClubByIdQuery(Guid.NewGuid()));

        result.Should().BeNull();
    }
}
