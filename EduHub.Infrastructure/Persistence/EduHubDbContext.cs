using EduHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

public class EduHubDbContext(DbContextOptions<EduHubDbContext> options) : DbContext(options)
{
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Member>(member =>
        {
            member.ToTable("members");
            member.Property(m => m.StudentId).IsRequired().HasMaxLength(20);
            member.Property(m => m.FullName).IsRequired().HasMaxLength(150);
            member.Property(m => m.Email).IsRequired().HasMaxLength(200);
            member.HasIndex(m => m.StudentId).IsUnique();
            member.HasIndex(m => m.Email).IsUnique();
        });

        modelBuilder.Entity<Club>(club =>
        {
            club.ToTable("clubs");
            club.Property(c => c.Name).IsRequired().HasMaxLength(150);
            club.Property(c => c.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<Event>(eventMapping =>
        {
            eventMapping.ToTable("events");
            eventMapping.Property(e => e.Name).IsRequired().HasMaxLength(200);
            eventMapping.Property(e => e.Description).HasMaxLength(2000);
            eventMapping.Property(e => e.Location).HasMaxLength(200);
            eventMapping.Property(e => e.Capacity).HasDefaultValue(100);
            eventMapping.HasOne<Club>().WithMany().HasForeignKey(e => e.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(registration =>
        {
            registration.ToTable("registrations");
            registration.Property(r => r.Status).IsRequired().HasMaxLength(20);
            registration.HasOne<Member>().WithMany().HasForeignKey(r => r.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
            registration.HasOne<Event>().WithMany().HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
            registration.HasIndex(r => new { r.MemberId, r.EventId }).IsUnique();
        });
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var transaction = await Database.BeginTransactionAsync(ct);
        // Giữ kiểm tra bảng trống và ghi dữ liệu trong cùng giao dịch khi nhiều API khởi động.
        await Database.ExecuteSqlRawAsync("LOCK TABLE clubs IN SHARE ROW EXCLUSIVE MODE", ct);
        if (await Clubs.AnyAsync(ct))
            return;

        var programming = Club.Create("CLB Lập trình", "Thực hành lập trình và phát triển ứng dụng.");
        var robotics = Club.Create("CLB Robotics", "Chế tạo và lập trình robot.");
        var languages = Club.Create("CLB Ngoại ngữ", "Giao lưu và luyện ngoại ngữ.");
        Clubs.AddRange(programming, robotics, languages);

        var today = DateTime.UtcNow.Date;
        Events.AddRange(
            Event.Create("Workshop .NET 10", programming.Id, today.AddDays(14).AddHours(2), 100,
                "Thực hành EF Core và PostgreSQL.", "Phòng B101"),
            Event.Create("Hackathon nhỏ", robotics.Id, today.AddDays(30).AddHours(2), 2,
                "Thi lập trình robot theo đội.", "Phòng B102"));

        var demoMembers = new[]
        {
            Member.Create("SV001", "Nguyễn Văn An", "an@example.test"),
            Member.Create("SV002", "Trần Thị Bình", "binh@example.test"),
            Member.Create("SV003", "Lê Minh Chi", "chi@example.test")
        };
        var studentIds = demoMembers.Select(m => m.StudentId).ToArray();
        var emails = demoMembers.Select(m => m.Email).ToArray();
        var existingMembers = await Members
            .Where(m => studentIds.Contains(m.StudentId) || emails.Contains(m.Email))
            .Select(m => new { m.StudentId, m.Email })
            .ToListAsync(ct);
        Members.AddRange(demoMembers.Where(demo => !existingMembers.Any(existing =>
            existing.StudentId == demo.StudentId || existing.Email == demo.Email)));

        await SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
