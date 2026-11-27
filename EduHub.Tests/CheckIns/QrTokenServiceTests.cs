using EduHub.Application.CheckIns;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EduHub.Tests.CheckIns;

public class QrTokenServiceTests
{
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Validate_FreshToken_IsValidAndReturnsEventId()
    {
        var service = CreateService();
        var eventId = Guid.NewGuid();
        var qr = service.Generate(eventId);

        Assert.Equal(QrValidation.Valid, service.Validate(qr.Payload, out var foundEventId));
        Assert.Equal(eventId, foundEventId);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime.AddMinutes(15), qr.ExpiresAt);
    }

    [Fact]
    public void Validate_After15Minutes_IsExpired()
    {
        var service = CreateService();
        var qr = service.Generate(Guid.NewGuid());
        _clock.Advance(TimeSpan.FromMinutes(15) - TimeSpan.FromSeconds(1));
        Assert.Equal(QrValidation.Valid, service.Validate(qr.Payload, out _));

        _clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(QrValidation.Expired, service.Validate(qr.Payload, out _));
        _clock.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal(QrValidation.Expired, service.Validate(qr.Payload, out _));
    }

    [Fact]
    public void Validate_TamperedBody_IsInvalid()
    {
        var service = CreateService();
        var first = service.Generate(Guid.NewGuid()).Payload.Split('.');
        var second = service.Generate(Guid.NewGuid()).Payload.Split('.');

        Assert.Equal(QrValidation.Invalid, service.Validate($"{first[0]}.{second[1]}", out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("bad!payload")]
    [InlineData("a.b.c")]
    [InlineData("abc.!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!")]
    public void Validate_MalformedPayload_IsInvalid(string payload) =>
        Assert.Equal(QrValidation.Invalid, CreateService().Validate(payload, out _));

    private QrTokenService CreateService() => new(
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["QrSettings:Secret"] = "unit-test-qr-placeholder-key-32-bytes"
        }).Build(), _clock);
}
