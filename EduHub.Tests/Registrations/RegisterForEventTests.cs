using EduHub.Application.Common;
using EduHub.Application.Registrations;
using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EduHub.Tests.Registrations;

public sealed class RegisterForEventTests : IDisposable
{
    private readonly Mock<IEventRepository> events = new(MockBehavior.Strict);
    private readonly Mock<IRegistrationRepository> registrations = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> unitOfWork = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWorkTransaction> transaction = new(MockBehavior.Strict);
    private readonly CancellationTokenSource cancellation = new();
    private readonly Guid memberId = Guid.NewGuid();
    private readonly Event demoEvent = Event.Create("Workshop EF Core", Guid.NewGuid(), DateTime.UtcNow.AddDays(1), 2);
    private readonly RegisterForEventCommandHandler handler;
    private CancellationToken Ct => cancellation.Token;

    public RegisterForEventTests()
    {
        unitOfWork.Setup(work => work.BeginTransactionAsync(Ct)).ReturnsAsync(transaction.Object);
        transaction.Setup(tx => tx.DisposeAsync()).Returns(ValueTask.CompletedTask);
        handler = new RegisterForEventCommandHandler(events.Object, registrations.Object, unitOfWork.Object,
            NullLogger<RegisterForEventCommandHandler>.Instance);
    }

    [Fact]
    public async Task Register_Valid_AddsAndSavesOnce()
    {
        SetUpExistingEventAndMember();
        registrations.Setup(repo => repo.ExistsAsync(memberId, demoEvent.Id, Ct)).ReturnsAsync(false);
        registrations.Setup(repo => repo.CountActiveByEventIdAsync(demoEvent.Id, Ct)).ReturnsAsync(1);
        registrations.Setup(repo => repo.Add(It.IsAny<Registration>()));
        unitOfWork.Setup(work => work.SaveChangesAsync(Ct)).ReturnsAsync(1);
        transaction.Setup(tx => tx.CommitAsync(Ct)).Returns(Task.CompletedTask);

        var result = await handler.HandleAsync(new RegisterForEventCommand(memberId, demoEvent.Id), Ct);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(memberId, result.MemberId);
        Assert.Equal(demoEvent.Id, result.EventId);
        Assert.Equal("Pending", result.Status);
        registrations.Verify(repo => repo.Add(It.Is<Registration>(r =>
            r.Id == result.Id && r.EventId == demoEvent.Id && r.MemberId == memberId)), Times.Once);
        unitOfWork.Verify(work => work.SaveChangesAsync(Ct), Times.Once);
        transaction.Verify(tx => tx.CommitAsync(Ct), Times.Once);
    }

    [Fact]
    public async Task Register_UnknownEvent_ThrowsNotFound()
    {
        var eventId = Guid.NewGuid();
        events.Setup(repo => repo.GetForUpdateAsync(eventId, Ct)).ReturnsAsync((Event?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new RegisterForEventCommand(memberId, eventId), Ct));
        VerifyNoWrite();
    }

    [Fact]
    public async Task Register_Duplicate_ThrowsConflict()
    {
        SetUpExistingEventAndMember();
        registrations.Setup(repo => repo.ExistsAsync(memberId, demoEvent.Id, Ct)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            handler.HandleAsync(new RegisterForEventCommand(memberId, demoEvent.Id), Ct));
        registrations.Verify(repo => repo.CountActiveByEventIdAsync(demoEvent.Id, Ct), Times.Never);
        VerifyNoWrite();
    }

    [Fact]
    public async Task Register_WhenFull_ThrowsBusinessRule()
    {
        SetUpExistingEventAndMember();
        registrations.Setup(repo => repo.ExistsAsync(memberId, demoEvent.Id, Ct)).ReturnsAsync(false);
        registrations.Setup(repo => repo.CountActiveByEventIdAsync(demoEvent.Id, Ct))
            .ReturnsAsync(demoEvent.Capacity);

        var error = await Assert.ThrowsAsync<BusinessRuleException>(() =>
            handler.HandleAsync(new RegisterForEventCommand(memberId, demoEvent.Id), Ct));

        Assert.Equal("Sự kiện đã đạt giới hạn.", error.Message);
        VerifyNoWrite();
    }

    private void SetUpExistingEventAndMember()
    {
        events.Setup(repo => repo.GetForUpdateAsync(demoEvent.Id, Ct)).ReturnsAsync(demoEvent);
        registrations.Setup(repo => repo.MemberExistsAsync(memberId, Ct)).ReturnsAsync(true);
    }

    private void VerifyNoWrite()
    {
        registrations.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
        unitOfWork.Verify(work => work.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        transaction.Verify(tx => tx.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        transaction.Verify(tx => tx.DisposeAsync(), Times.Once);
    }

    public void Dispose() => cancellation.Dispose();
}
