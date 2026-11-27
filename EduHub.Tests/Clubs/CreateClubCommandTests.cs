using System.ComponentModel.DataAnnotations;
using EduHub.Application.Clubs;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Clubs;

public class CreateClubCommandTests
{
    [Theory]
    [InlineData("Sinh hoạt hằng tuần", "Sinh hoạt hằng tuần")]
    [InlineData(null, "")]
    public async Task CreateClub_WithValidName_SavesAndReturnsClub(string? description, string expectedDescription)
    {
        var repo = new Mock<IClubRepository>();
        var handler = new CreateClubCommandHandler(repo.Object);
        using var cancellation = new CancellationTokenSource();

        var result = await handler.HandleAsync(new CreateClubCommand("  CLB Robotics  ", description), cancellation.Token);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("CLB Robotics");
        result.Description.Should().Be(expectedDescription);
        repo.Verify(r => r.AddAsync(result, cancellation.Token), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateClub_WhenNameIsBlank_ThrowsValidationException(string name)
    {
        var repo = new Mock<IClubRepository>();
        var handler = new CreateClubCommandHandler(repo.Object);

        var create = () => handler.HandleAsync(new CreateClubCommand(name, null));

        await create.Should().ThrowAsync<ValidationException>();
        repo.Verify(r => r.AddAsync(It.IsAny<Club>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
