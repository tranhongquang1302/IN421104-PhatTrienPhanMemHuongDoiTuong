using EduHub.Application.CheckIns;
using EduHub.Infrastructure.Auth;
using EduHub.Tests.Support;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace EduHub.Tests.CheckIns;

public class QrTokenServiceTests
{
    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 12, 8, 0, 0, TimeSpan.Zero));

    // Gợi ý tạo service: new QrTokenService(config, _clock) với config từ
    // new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["QrSettings:Secret"] = "..." }).Build()

    [Fact]
    public void Validate_FreshToken_IsValidAndReturnsEventId()
    {
        // TODO 6.7: Generate(eventId) rồi Validate => QrValidation.Valid và eventId khớp
        throw new NotImplementedException();
    }

    [Fact]
    public void Validate_After15Minutes_IsExpired()
    {
        // TODO 6.8: _clock.Advance(TimeSpan.FromMinutes(15) + TimeSpan.FromSeconds(1)) => QrValidation.Expired
        throw new NotImplementedException();
    }

    [Fact]
    public void Validate_TamperedBody_IsInvalid()
    {
        // TODO 6.9: ghép phần nội dung của QR này với chữ ký của QR khác => QrValidation.Invalid
        throw new NotImplementedException();
    }
}
