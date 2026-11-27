using EduHub.Application.Abstractions;
using EduHub.Application.Auth;
using EduHub.Application.CheckIns;
using EduHub.Domain.Interfaces;
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
        var user = new AppUser
        {
            UserName = email.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            DisplayName = displayName.Trim()
        };
        // Identity kiểm tra độ mạnh mật khẩu + trùng email, rồi gọi hasher Argon2id để băm
        var created = await users.CreateAsync(user, password);
        if (!created.Succeeded)
            return new RegisterResult(null, created.Errors.Select(e => e.Description).ToList());

        await users.AddToRoleAsync(user, Roles.Member);
        return new RegisterResult(await ToAccountAsync(user), []);
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
    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default) =>
        await db.RefreshTokens.AddAsync(token, ct);

    public async Task RevokeAllAsync(Guid userId, DateTime utcNow, CancellationToken ct = default)
    {
        var active = await db.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync(ct);
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

public class ClubRepository(EduHubDbContext db) : IClubRepository
{
    public async Task<IEnumerable<Club>> GetAllAsync(CancellationToken ct = default)
        => await db.Clubs.AsNoTracking().OrderBy(c => c.Name).ToListAsync(ct);

    public Task<Club?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Clubs.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task AddAsync(Club club, CancellationToken ct = default)
    {
        db.Clubs.Add(club);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var club = await db.Clubs.FindAsync([id], ct);
        if (club is null) return false;
        db.Clubs.Remove(club);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

public class EventRepository(EduHubDbContext db) : IEventRepository
{
    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken ct = default)
        => await db.Events.AsNoTracking().OrderBy(e => e.StartTime).ToListAsync(ct);

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AddAsync(Event ev, CancellationToken ct = default)
    {
        db.Events.Add(ev);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var ev = await db.Events.FindAsync([id], ct);
        if (ev is null) return false;
        db.Events.Remove(ev);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

public class RegistrationRepository(EduHubDbContext db) : IRegistrationRepository
{
    public Task<int> CountByEventAsync(Guid eventId, CancellationToken ct = default)
        => db.Registrations.CountAsync(r => r.EventId == eventId && r.Status != "Cancelled", ct);

    public Task<bool> ExistsAsync(Guid eventId, Guid memberId, CancellationToken ct = default)
        => db.Registrations.AnyAsync(r => r.EventId == eventId && r.MemberId == memberId, ct);

    public async Task AddAsync(Registration registration, CancellationToken ct = default)
    {
        db.Registrations.Add(registration);
        await db.SaveChangesAsync(ct);
    }
}
