using System.Security.Claims;
using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

/// <summary>
/// Dữ liệu mẫu cho demo: 2 vai trò, 3 người dùng (Admin, Member, Member có claim qr.issue), 1 CLB, 1 sự kiện.
/// Chỉ chạy khi DB trống. Mật khẩu được băm bằng Argon2id qua Identity.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        EduHubDbContext db, UserManager<AppUser> users, RoleManager<AppRole> roles)
    {
        await db.Database.EnsureCreatedAsync();
        if (await db.Users.AnyAsync()) return;

        foreach (var role in new[] { Roles.Admin, Roles.Member })
            await roles.CreateAsync(new AppRole(role));

        await CreateAsync(users, "admin@eduhub.local", "Quản trị viên", "Admin@123", Roles.Admin);
        await CreateAsync(users, "member@eduhub.local", "Nguyễn Văn A", "Member@123", Roles.Member);
        var organizer = await CreateAsync(users, "organizer@eduhub.local", "Trần Thị B (ban tổ chức)", "Organizer@123", Roles.Member);
        // Claim bổ sung: Member này được phép tạo QR điểm danh dù không phải Admin
        await users.AddClaimAsync(organizer, new Claim(AppClaims.Permission, AppClaims.QrIssue));

        var club = Club.Create("CLB Lập trình", "Sinh hoạt công nghệ hằng tuần");
        var ev = Event.Create("Workshop JWT", club.Id, DateTime.UtcNow.AddDays(3), 50);
        db.AddRange(club, ev);
        await db.SaveChangesAsync();
    }

    private static async Task<AppUser> CreateAsync(
        UserManager<AppUser> users, string email, string name, string password, string role)
    {
        var user = new AppUser { UserName = email, Email = email, DisplayName = name, EmailConfirmed = true };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("Seed thất bại: " + string.Join("; ", result.Errors.Select(e => e.Description)));
        await users.AddToRoleAsync(user, role);
        return user;
    }
}
