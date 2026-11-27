using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Persistence;

// IdentityDbContext thêm sẵn các bảng người dùng, vai trò, claim (AspNetUsers, AspNetRoles, ...)
public class EduHubDbContext(DbContextOptions<EduHubDbContext> options)
    : IdentityDbContext<AppUser, AppRole, Guid>(options)
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);   // bắt buộc: cấu hình các bảng Identity

        modelBuilder.Entity<AppUser>(b => b.Property(x => x.DisplayName).HasMaxLength(200));

        modelBuilder.Entity<Club>(b =>
        {
            b.ToTable("clubs");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Event>(b =>
        {
            b.ToTable("events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(200).IsRequired();
            b.HasOne<Club>().WithMany().HasForeignKey(x => x.ClubId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Registration>(b =>
        {
            b.ToTable("registrations");
            b.HasKey(x => x.Id);
            b.Property(x => x.Status).HasMaxLength(20);
            b.HasOne<AppUser>().WithMany().HasForeignKey(x => x.MemberId).OnDelete(DeleteBehavior.Cascade);
            b.HasOne<Event>().WithMany().HasForeignKey(x => x.EventId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x => new { x.MemberId, x.EventId }).IsUnique();
        });

        modelBuilder.Entity<CheckIn>(b =>
        {
            b.ToTable("checkins");
            b.HasKey(x => x.Id);
            b.HasOne<Registration>().WithMany().HasForeignKey(x => x.RegistrationId).OnDelete(DeleteBehavior.Cascade);
            // Mỗi đăng ký chỉ check-in 1 lần — chặn trùng ở cả mức DB
            b.HasIndex(x => x.RegistrationId).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            b.HasIndex(x => x.TokenHash).IsUnique();
            b.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
