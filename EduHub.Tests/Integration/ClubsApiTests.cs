using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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

    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder
        .UseSetting("ConnectionStrings:DefaultConnection", "")
        .UseSetting("JwtSettings:Secret", "chuoi-bi-mat-chi-dung-cho-test-it-nhat-32-ky-tu");

    public async Task InitializeAsync()
    {
        AdminToken = await LoginAsync("admin@eduhub.local", "Admin@123");
        MemberToken = await LoginAsync("member@eduhub.local", "Member@123");
    }

    Task IAsyncLifetime.DisposeAsync() => base.DisposeAsync().AsTask();

    private async Task<string> LoginAsync(string email, string password)
    {
        using var client = CreateClient();
        using var res = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await res.Content.ReadFromJsonAsync<LoginBody>();
        login.Should().NotBeNull();
        login!.AccessToken.Should().NotBeNullOrWhiteSpace();
        return login.AccessToken;
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
    [Fact]
    public async Task GetClubs_AsMember_Returns200WithSeededClub()
    {
        using var client = factory.ClientWith(factory.MemberToken);

        using var response = await client.GetAsync("/api/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clubs = await response.Content.ReadFromJsonAsync<ClubBody[]>();
        clubs.Should().Contain(club => club.Name == "CLB Lập trình" && club.Id != Guid.Empty);
    }

    [Fact]
    public async Task PostClub_AsAdmin_Returns201AndLocationOfSavedClub()
    {
        using var client = factory.ClientWith(factory.AdminToken);
        var request = new { name = "CLB Robotics", description = "Chế tạo robot" };

        using var response = await client.PostAsJsonAsync("/api/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ClubBody>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Name.Should().Be(request.name);
        created.Description.Should().Be(request.description);
        var location = response.Headers.Location;
        location.Should().NotBeNull();
        new Uri(client.BaseAddress!, location!).AbsolutePath.Should().BeEquivalentTo($"/api/clubs/{created.Id}");

        using var savedResponse = await client.GetAsync(location);
        savedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var saved = await savedResponse.Content.ReadFromJsonAsync<ClubBody>();
        saved.Should().Be(created);
    }

    [Fact]
    public async Task GetClubById_WhenMissing_Returns404()
    {
        using var client = factory.ClientWith(factory.MemberToken);

        using var response = await client.GetAsync($"/api/clubs/{Guid.Empty}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetClubs_WithoutToken_Returns401()
    {
        using var client = factory.ClientWith(null);

        using var response = await client.GetAsync("/api/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostClub_AsMember_Returns403()
    {
        using var client = factory.ClientWith(factory.MemberToken);

        using var response = await client.PostAsJsonAsync("/api/clubs", new { name = "CLB Robotics" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private record ClubBody(Guid Id, string Name, string Description);
}
