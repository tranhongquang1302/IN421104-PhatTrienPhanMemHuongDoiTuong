using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Application.CheckIns;
using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

/// <summary>Đọc/tạo người dùng qua UserManager của ASP.NET Core Identity.</summary>
public class AccountRepository(UserManager<AppUser> users) : IAccountRepository
{
    public async Task<Account?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        await ToAccountAsync(await users.FindByEmailAsync(email));

    public async Task<Account?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        await ToAccountAsync(await users.FindByIdAsync(id.ToString()));

    public async Task<RegisterResult> CreateMemberAsync(
        string email, string displayName, string password, CancellationToken ct = default)
    {
        // TODO 3.1: tạo AppUser (UserName = Email = email chuẩn hoá chữ thường, DisplayName)
        // TODO 3.2: users.CreateAsync(user, password) — Identity kiểm tra độ mạnh mật khẩu + trùng email,
        //           và gọi hasher Argon2id để băm. Thất bại => RegisterResult(null, danh sách lỗi)
        // TODO 3.3: users.AddToRoleAsync(user, Roles.Member); trả RegisterResult(account, [])
        throw new NotImplementedException();
    }

    private async Task<Account?> ToAccountAsync(AppUser? user)
    {
        if (user is null) return null;
        var roles = await users.GetRolesAsync(user);
        var claims = (await users.GetClaimsAsync(user)).Select(c => new UserClaim(c.Type, c.Value)).ToList();
        return new Account(user.Id, user.Email!, user.DisplayName, user.PasswordHash ?? "", roles.ToList(), claims);
    }
}

public class RefreshTokenStore(EduHubDbContext db) : IRefreshTokenStore
{
    public async Task<TResult> WithTokenAsync<TResult>(
        string tokenHash, Func<RefreshToken?, Task<TResult>> action, CancellationToken ct = default)
    {
        var userId = await db.RefreshTokens
            .Where(t => t.TokenHash == tokenHash)
            .Select(t => (Guid?)t.UserId)
            .FirstOrDefaultAsync(ct);
        if (userId is null)
            return await action(null);

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Khóa theo tài khoản trước khi đọc token, để xoay vòng và thu hồi không chạy chồng nhau.
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""SELECT 1 FROM "AspNetUsers" WHERE "Id" = {userId.Value} FOR NO KEY UPDATE""", ct);
        var token = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
        var result = await action(token);
        await transaction.CommitAsync(ct);
        return result;
    }

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllAsync(Guid userId, DateTime utcNow, CancellationToken ct = default)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > utcNow)
            .ToListAsync(ct);
        foreach (var t in active) t.Revoke(utcNow);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}

public class CheckInRepository(EduHubDbContext db) : ICheckInRepository
{
    public Task<bool> EventExistsAsync(Guid eventId, CancellationToken ct = default) =>
        db.Events.AnyAsync(e => e.Id == eventId, ct);

    public Task<Registration?> FindRegistrationAsync(Guid memberId, Guid eventId, CancellationToken ct = default) =>
        db.Registrations.FirstOrDefaultAsync(r => r.MemberId == memberId && r.EventId == eventId, ct);

    public Task<bool> CheckInExistsAsync(Guid registrationId, CancellationToken ct = default) =>
        db.CheckIns.AnyAsync(c => c.RegistrationId == registrationId, ct);

    public async Task AddAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        db.CheckIns.Add(checkIn);
        await db.SaveChangesAsync(ct);
    }
}
