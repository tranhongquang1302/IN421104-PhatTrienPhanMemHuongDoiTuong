using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EduHub.Tests.Integration;

/// <summary>
/// Không đặt connection string -> API dùng EF Core in-memory, không cần PostgreSQL.
/// Secret JWT truyền qua UseSetting; dữ liệu mẫu (admin, member) do DbSeeder tạo khi khởi động.
/// Đăng nhập một lần cho cả lớp test (endpoint /api/auth/login bị rate limit 10 lần/phút).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public string AdminToken { get; private set; } = "";
    public string MemberToken { get; private set; } = "";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        => builder.UseSetting("JwtSettings:Secret", "chuoi-bi-mat-chi-dung-cho-test-it-nhat-32-ky-tu");

    public async Task InitializeAsync()
    {
        AdminToken = await LoginAsync("admin@eduhub.local", "Admin@123");
        MemberToken = await LoginAsync("member@eduhub.local", "Member@123");
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private async Task<string> LoginAsync(string email, string password)
    {
        var res = await CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await res.Content.ReadFromJsonAsync<LoginBody>())!.AccessToken;
    }

    public HttpClient ClientWith(string? token)
    {
        var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private record LoginBody(string AccessToken);
}

public class ClubsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // factory.ClientWith(null)                  -> client chưa đăng nhập
    // factory.ClientWith(factory.AdminToken)    -> client đã đăng nhập Admin (token lấy sẵn từ tài khoản mẫu)
    // factory.ClientWith(factory.MemberToken)   -> client đã đăng nhập Member

    // TODO: GetClubs_WithToken_Returns200          -> GET /api/clubs (Member)   => 200 OK
    [Fact(Skip = "TODO: viết test")]
    public Task GetClubs_WithToken_Returns200() => Task.CompletedTask;

    // TODO: PostClub_AsAdminWithValidBody_Returns201AndClub -> POST /api/clubs (Admin) => 201 Created + club mới
    [Fact(Skip = "TODO: viết test")]
    public Task PostClub_AsAdminWithValidBody_Returns201AndClub() => Task.CompletedTask;

    // TODO: GetClubById_WhenNotExists_Returns404   -> GET /api/clubs/{id} với Guid ngẫu nhiên => 404
    [Fact(Skip = "TODO: viết test")]
    public Task GetClubById_WhenNotExists_Returns404() => Task.CompletedTask;
}
