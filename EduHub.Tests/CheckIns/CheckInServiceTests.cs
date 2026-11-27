using EduHub.Application.CheckIns;
using EduHub.Domain.Entities;
using EduHub.Tests.Support;
using Moq;
using Xunit;

namespace EduHub.Tests.CheckIns;

public class CheckInServiceTests
{
    private readonly Guid _eventId = Guid.NewGuid();
    private readonly Guid _memberId = Guid.NewGuid();
    private readonly Mock<IQrTokenService> _qr = new(MockBehavior.Strict);
    private readonly Mock<ICheckInRepository> _repository = new(MockBehavior.Strict);
    private readonly ManualClock _clock = new(TestAuth.Start);

    [Theory]
    [InlineData(false, QrValidation.Valid, true, CheckInOutcome.EventNotFound)]
    [InlineData(true, QrValidation.Invalid, true, CheckInOutcome.InvalidQr)]
    [InlineData(true, QrValidation.Expired, false, CheckInOutcome.ExpiredQr)]
    [InlineData(true, QrValidation.Valid, false, CheckInOutcome.InvalidQr)]
    public async Task CheckIn_RejectsMissingEventOrUnusableQr(bool eventExists, QrValidation validation,
        bool eventMatches, CheckInOutcome expected)
    {
        _repository.Setup(r => r.EventExistsAsync(_eventId, default)).ReturnsAsync(eventExists);
        if (eventExists)
        {
            var qrEventId = eventMatches ? _eventId : Guid.NewGuid();
            _qr.Setup(q => q.Validate("qr", out qrEventId)).Returns(validation);
        }

        var result = await CreateService().CheckInAsync(_memberId, _eventId, "qr");

        Assert.Equal(expected, result.Outcome);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CheckIn_RequiresAnActiveRegistration(bool cancelled)
    {
        var registration = cancelled ? Registration.Create(_memberId, _eventId) : null;
        registration?.Cancel();
        SetupRegistration(registration);

        var result = await CreateService().CheckInAsync(_memberId, _eventId, "qr");

        Assert.Equal(CheckInOutcome.NotRegistered, result.Outcome);
    }

    [Theory]
    [InlineData(true, false, CheckInOutcome.AlreadyCheckedIn)]
    [InlineData(false, false, CheckInOutcome.AlreadyCheckedIn)]
    [InlineData(false, true, CheckInOutcome.Success)]
    public async Task CheckIn_SavesOneVerifiedCheckInPerRegistration(bool alreadyExists,
        bool insertSucceeded, CheckInOutcome expected)
    {
        var registration = Registration.Create(_memberId, _eventId);
        registration.Confirm();
        SetupRegistration(registration);
        _repository.Setup(r => r.CheckInExistsAsync(registration.Id, default)).ReturnsAsync(alreadyExists);
        if (!alreadyExists)
        {
            _repository.Setup(r => r.TryAddAsync(It.Is<CheckIn>(entry => entry.RegistrationId == registration.Id &&
                entry.IsVerified && entry.CheckedInAt == _clock.GetUtcNow().UtcDateTime), default))
                .ReturnsAsync(insertSucceeded);
        }

        var result = await CreateService().CheckInAsync(_memberId, _eventId, "qr");

        Assert.Equal(expected, result.Outcome);
        Assert.Equal(insertSucceeded ? _clock.GetUtcNow().UtcDateTime : (DateTime?)null, result.CheckedInAt);
    }

    private CheckInService CreateService() => new(_qr.Object, _repository.Object, _clock);

    private void SetupRegistration(Registration? registration)
    {
        var qrEventId = _eventId;
        _qr.Setup(q => q.Validate("qr", out qrEventId)).Returns(QrValidation.Valid);
        _repository.Setup(r => r.EventExistsAsync(_eventId, default)).ReturnsAsync(true);
        _repository.Setup(r => r.FindRegistrationAsync(_memberId, _eventId, default)).ReturnsAsync(registration);
    }
}
