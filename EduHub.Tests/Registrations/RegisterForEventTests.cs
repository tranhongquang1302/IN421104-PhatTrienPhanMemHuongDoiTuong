using EduHub.Application.Registrations;
using EduHub.Domain.Entities;
using EduHub.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace EduHub.Tests.Registrations;

public class RegisterForEventTests
{
    private readonly Event eventInfo = Event.Create(
        "Workshop", Guid.NewGuid(), new DateTime(2026, 12, 12, 9, 0, 0, DateTimeKind.Utc), 2);
    private readonly Guid memberId = Guid.NewGuid();
    private readonly Mock<IEventRepository> events = new();
    private readonly Mock<IRegistrationRepository> registrations = new();

    [Fact]
    public async Task Register_WhenOneSeatRemains_SavesConfirmedRegistration()
    {
        events.Setup(r => r.GetByIdAsync(eventInfo.Id, default)).ReturnsAsync(eventInfo);
        registrations.Setup(r => r.ExistsAsync(eventInfo.Id, memberId, default)).ReturnsAsync(false);
        registrations.Setup(r => r.CountByEventAsync(eventInfo.Id, default)).ReturnsAsync(eventInfo.Capacity - 1);
        var handler = new RegisterForEventCommandHandler(events.Object, registrations.Object);

        var result = await handler.HandleAsync(new RegisterForEventCommand(eventInfo.Id, memberId));

        result.Id.Should().NotBeEmpty();
        result.EventId.Should().Be(eventInfo.Id);
        result.MemberId.Should().Be(memberId);
        result.Status.Should().Be("Confirmed");
        registrations.Verify(r => r.AddAsync(result, default), Times.Once);
    }

    [Fact]
    public async Task Register_WhenEventIsAtCapacity_ThrowsInvalidOperationException()
    {
        events.Setup(r => r.GetByIdAsync(eventInfo.Id, default)).ReturnsAsync(eventInfo);
        registrations.Setup(r => r.ExistsAsync(eventInfo.Id, memberId, default)).ReturnsAsync(false);
        registrations.Setup(r => r.CountByEventAsync(eventInfo.Id, default)).ReturnsAsync(eventInfo.Capacity);
        var handler = new RegisterForEventCommandHandler(events.Object, registrations.Object);

        var register = () => handler.HandleAsync(new RegisterForEventCommand(eventInfo.Id, memberId));

        await register.Should().ThrowAsync<InvalidOperationException>().WithMessage("Sự kiện đã đủ chỗ.");
        registrations.Verify(r => r.AddAsync(It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenAlreadyRegistered_ThrowsBeforeCheckingCapacity()
    {
        events.Setup(r => r.GetByIdAsync(eventInfo.Id, default)).ReturnsAsync(eventInfo);
        registrations.Setup(r => r.ExistsAsync(eventInfo.Id, memberId, default)).ReturnsAsync(true);
        var handler = new RegisterForEventCommandHandler(events.Object, registrations.Object);

        var register = () => handler.HandleAsync(new RegisterForEventCommand(eventInfo.Id, memberId));

        await register.Should().ThrowAsync<InvalidOperationException>().WithMessage("Thành viên đã đăng ký sự kiện này.");
        registrations.Verify(r => r.CountByEventAsync(eventInfo.Id, default), Times.Never);
        registrations.Verify(r => r.AddAsync(It.IsAny<Registration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenEventNotFound_ThrowsWithoutCheckingRegistrations()
    {
        events.Setup(r => r.GetByIdAsync(eventInfo.Id, default)).ReturnsAsync((Event?)null);
        var handler = new RegisterForEventCommandHandler(events.Object, registrations.Object);

        var register = () => handler.HandleAsync(new RegisterForEventCommand(eventInfo.Id, memberId));

        await register.Should().ThrowAsync<KeyNotFoundException>();
        registrations.VerifyNoOtherCalls();
    }
}
