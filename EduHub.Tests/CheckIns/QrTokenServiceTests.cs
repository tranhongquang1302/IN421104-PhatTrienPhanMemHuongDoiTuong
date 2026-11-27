using EduHub.Application.CheckIns;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EduHub.Tests.CheckIns;

public class QrTokenServiceTests
{
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero));

    private QrTokenService Build(string secret = "bi-mat-dung-de-ky-qr-it-nhat-32-ky-tu-1234") =>
        new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["QrSettings:Secret"] = secret }).Build(), _clock);

    [Fact]
    public void Validate_FreshToken_IsValidAndReturnsEventId()
    {
        var qr = Build();
        var eventId = Guid.NewGuid();

        var payload = qr.Generate(eventId);

        Assert.Equal(QrValidation.Valid, qr.Validate(payload.Payload, out var got));
        Assert.Equal(eventId, got);
        Assert.Equal(_clock.GetUtcNow().UtcDateTime.AddMinutes(15), payload.ExpiresAt);
    }

    [Fact]
    public void Validate_After15Minutes_IsExpired()
    {
        var qr = Build();
        var payload = qr.Generate(Guid.NewGuid());

        _clock.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1));

        Assert.Equal(QrValidation.Expired, qr.Validate(payload.Payload, out _));
    }

    [Fact]
    public void Validate_TamperedBody_IsInvalid()
    {
        var qr = Build();
        var real = qr.Generate(Guid.NewGuid()).Payload;
        var forged = qr.Generate(Guid.NewGuid()).Payload;

        // Ghép phần nội dung của QR khác với chữ ký của QR này => chữ ký không khớp
        var tampered = forged.Split('.')[0] + "." + real.Split('.')[1];

        Assert.Equal(QrValidation.Invalid, qr.Validate(tampered, out _));
    }

    [Fact]
    public void Validate_SignedWithOtherSecret_IsInvalid()
    {
        var payload = Build("mot-bi-mat-khac-hoan-toan-it-nhat-32-ky-tu-99").Generate(Guid.NewGuid());

        Assert.Equal(QrValidation.Invalid, Build().Validate(payload.Payload, out _));
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc.def")]
    [InlineData("khong-co-dau-cham")]
    [InlineData("a.b.c")]
    public void Validate_Garbage_IsInvalid(string garbage) =>
        Assert.Equal(QrValidation.Invalid, Build().Validate(garbage, out _));
}
