using EduHub.Application.Members;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using Moq;

namespace EduHub.Tests.Members;

public class CreateMemberCommandTests
{
    [Fact]
    public async Task CreateMember_WithValidData_ReturnsCreatedMember()
    {
        // Arrange
        var mockRepo = new Mock<IMemberRepository>();
        var handler = new CreateMemberCommandHandler(mockRepo.Object);
        var command = new CreateMemberCommand("SV001", "Trần Hồng Quang", "0024419387@student.dthu.edu.vn");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Trần Hồng Quang", result.Name);
        Assert.Equal("0024419387@student.dthu.edu.vn", result.Email);
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Member>(), default), Times.Once);
    }

    [Fact]
    public async Task CreateMember_WithEmptyName_ThrowsException()
    {
        var mockRepo = new Mock<IMemberRepository>();
        var handler = new CreateMemberCommandHandler(mockRepo.Object);
        var command = new CreateMemberCommand("SV001", "", "email@dthu.edu.vn");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Member>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateMember_WithEmptyEmail_ThrowsException()
    {
        var mockRepo = new Mock<IMemberRepository>();
        var handler = new CreateMemberCommandHandler(mockRepo.Object);
        var command = new CreateMemberCommand("SV001", "Trần Hồng Quang", "");

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(command));
        mockRepo.Verify(r => r.AddAsync(It.IsAny<Member>(), default), Times.Never);
    }
}

public class GetMembersQueryTests
{
    [Fact]
    public async Task GetMembers_WhenEmpty_ReturnsEmptyList()
    {
        var mockRepo = new Mock<IMemberRepository>();
        mockRepo.Setup(r => r.GetAllAsync(default))
                .ReturnsAsync(Enumerable.Empty<Member>());
        var handler = new GetMembersQueryHandler(mockRepo.Object);

        var result = await handler.HandleAsync(new GetMembersQuery());

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

public class GetMemberByIdQueryTests
{
    [Fact]
    public async Task GetMemberById_WhenIdExists_ReturnsCorrectMember()
    {
        var member = Member.Create("SV001", "Trần Hồng Quang", "0024419387@student.dthu.edu.vn");
        var mockRepo = new Mock<IMemberRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(member.Id, default)).ReturnsAsync(member);
        var handler = new GetMemberByIdQueryHandler(mockRepo.Object);

        var result = await handler.HandleAsync(new GetMemberByIdQuery(member.Id));

        Assert.NotNull(result);
        Assert.Equal(member.Id, result!.Id);
        Assert.Equal("Trần Hồng Quang", result.Name);
    }
}
