using System.Security.Claims;
using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

/// <summary>
/// Dữ liệu mẫu cho demo: 2 vai trò, 3 người dùng (Admin, Member, Member có claim qr.issue), 1 CLB, 1 sự kiện.
/// Tài khoản và sự kiện mẫu chỉ được tạo trên DB trống trong Development.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(
        EduHubDbContext db, UserManager<AppUser> users, RoleManager<AppRole> roles, bool seedDemoData)
    {
        await db.Database.EnsureCreatedAsync();

        foreach (var role in new[] { Roles.Admin, Roles.Member })
        {
            if (await roles.RoleExistsAsync(role)) continue;
            var result = await roles.CreateAsync(new AppRole(role));
            if (!result.Succeeded)
                throw new InvalidOperationException($"Không tạo được vai trò {role}.");
        }

        if (!seedDemoData || await db.Users.AnyAsync()) return;
        await using var transaction = await db.Database.BeginTransactionAsync();

        await CreateAsync(users, "admin@eduhub.local", "Quản trị viên", "Admin@123", Roles.Admin);
        await CreateAsync(users, "member@eduhub.local", "Nguyễn Văn A", "Member@123", Roles.Member);
        var organizer = await CreateAsync(users, "organizer@eduhub.local", "Trần Thị B (ban tổ chức)", "Organizer@123", Roles.Member);
        // Claim bổ sung: Member này được phép tạo QR điểm danh dù không phải Admin
        EnsureSuccess(await users.AddClaimAsync(organizer, new Claim(AppClaims.Permission, AppClaims.QrIssue)));

        var club = Club.Create("CLB Lập trình", "Sinh hoạt công nghệ hằng tuần");
        var ev = Event.Create("Workshop JWT", club.Id, DateTime.UtcNow.AddDays(3), 50);
        db.AddRange(club, ev);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private static async Task<AppUser> CreateAsync(
        UserManager<AppUser> users, string email, string name, string password, string role)
    {
        var user = new AppUser { UserName = email, Email = email, DisplayName = name, EmailConfirmed = true };
        EnsureSuccess(await users.CreateAsync(user, password));
        EnsureSuccess(await users.AddToRoleAsync(user, role));
        return user;
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException("Seed thất bại: " + string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}
