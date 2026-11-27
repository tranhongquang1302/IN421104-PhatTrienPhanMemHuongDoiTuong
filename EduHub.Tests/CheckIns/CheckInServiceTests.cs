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
    private readonly Mock<IQrTokenService> _qr = new();
    private readonly Mock<ICheckInRepository> _repo = new();
    private readonly CheckInService _service;

    public CheckInServiceTests()
    {
        _repo.Setup(r => r.EventExistsAsync(_eventId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _service = new CheckInService(_qr.Object, _repo.Object, new ManualClock(DateTimeOffset.UtcNow));
    }

    private void QrIs(QrValidation result, Guid? eventId = null)
    {
        var id = eventId ?? _eventId;
        _qr.Setup(q => q.Validate("qr", out id)).Returns(result);
    }

    private Registration Registered()
    {
        var reg = Registration.Create(_memberId, _eventId);
        reg.Confirm();
        _repo.Setup(r => r.FindRegistrationAsync(_memberId, _eventId, It.IsAny<CancellationToken>())).ReturnsAsync(reg);
        return reg;
    }

    [Fact]
    public async Task CheckIn_ValidQrAndRegistered_Succeeds()
    {
        QrIs(QrValidation.Valid);
        Registered();

        var result = await _service.CheckInAsync(_memberId, _eventId, "qr");

        Assert.Equal(CheckInOutcome.Success, result.Outcome);
        _repo.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckIn_ExpiredQr_IsRejected()
    {
        QrIs(QrValidation.Expired);

        Assert.Equal(CheckInOutcome.ExpiredQr, (await _service.CheckInAsync(_memberId, _eventId, "qr")).Outcome);
    }

    [Fact]
    public async Task CheckIn_QrOfAnotherEvent_IsRejected()
    {
        QrIs(QrValidation.Valid, eventId: Guid.NewGuid());   // QR hợp lệ nhưng của sự kiện khác

        Assert.Equal(CheckInOutcome.InvalidQr, (await _service.CheckInAsync(_memberId, _eventId, "qr")).Outcome);
    }

    [Fact]
    public async Task CheckIn_NotRegistered_IsRejected()
    {
        QrIs(QrValidation.Valid);

        Assert.Equal(CheckInOutcome.NotRegistered, (await _service.CheckInAsync(_memberId, _eventId, "qr")).Outcome);
    }

    [Fact]
    public async Task CheckIn_Twice_ReturnsAlreadyCheckedIn()
    {
        QrIs(QrValidation.Valid);
        var reg = Registered();
        _repo.Setup(r => r.CheckInExistsAsync(reg.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Assert.Equal(CheckInOutcome.AlreadyCheckedIn, (await _service.CheckInAsync(_memberId, _eventId, "qr")).Outcome);
        _repo.Verify(r => r.AddAsync(It.IsAny<CheckIn>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
