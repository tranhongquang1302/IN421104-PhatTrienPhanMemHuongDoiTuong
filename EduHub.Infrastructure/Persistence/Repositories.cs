using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Application.CheckIns;
using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EduHub.Infrastructure.Persistence;

/// <summary>Đọc/tạo người dùng qua UserManager của ASP.NET Core Identity.</summary>
public class AccountRepository(UserManager<AppUser> users, EduHubDbContext db) : IAccountRepository
{
    public async Task<Account?> FindByEmailAsync(string email, CancellationToken ct = default) =>
        await ToAccountAsync(await users.FindByEmailAsync(email));

    public async Task<Account?> FindByIdAsync(Guid id, CancellationToken ct = default) =>
        await ToAccountAsync(await users.FindByIdAsync(id.ToString()));

    public async Task<RegisterResult> CreateMemberAsync(
        string email, string displayName, string password, CancellationToken ct = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = new AppUser { UserName = normalizedEmail, Email = normalizedEmail, DisplayName = displayName.Trim() };
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await users.CreateAsync(user, password);
            if (!result.Succeeded)
                return new(null, result.Errors.Select(e => e.Description).ToArray());

            result = await users.AddToRoleAsync(user, Roles.Member);
            if (!result.Succeeded)
                return new(null, result.Errors.Select(e => e.Description).ToArray());

            var account = await ToAccountAsync(user);
            await transaction.CommitAsync(ct);
            return new(account, []);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: "UserNameIndex" })
        {
            return new(null, ["Email đã được đăng ký."]);
        }
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

    public async Task<bool> TryAddAsync(CheckIn checkIn, CancellationToken ct = default)
    {
        db.CheckIns.Add(checkIn);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: EduHubDbContext.UniqueCheckInIndex })
        {
            db.Entry(checkIn).State = EntityState.Detached;
            return false;
        }
    }
}
