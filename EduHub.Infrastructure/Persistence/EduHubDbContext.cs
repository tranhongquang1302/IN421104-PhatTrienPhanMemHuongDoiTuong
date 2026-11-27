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
        // TODO [Nhiệm vụ 1] Cấu hình bằng Fluent API:
        //  - Member: bảng "members"; StudentId (tối đa 20, bắt buộc, unique); FullName (150, bắt buộc);
        //            Email (200, bắt buộc, UNIQUE INDEX)
        //  - Club:   bảng "clubs"; Name (150, bắt buộc); Description (1000)
        //  - Event:  bảng "events"; Name (200, bắt buộc); Capacity có giá trị mặc định 100;
        //            khóa ngoại ClubId -> clubs.Id (xóa Club thì xóa Event)
        //  - Registration: bảng "registrations"; Status (20, bắt buộc); khóa ngoại MemberId, EventId;
        //            UNIQUE INDEX tổ hợp (MemberId, EventId) để một thành viên chỉ đăng ký một lần
        // Gợi ý: modelBuilder.Entity<Member>(b => { b.ToTable("members"); ... });
    }

    // TODO [Nhiệm vụ 1] Seed: 3 câu lạc bộ, 2 sự kiện (một sự kiện sức chứa 2 để demo "đầy"), 3 thành viên.
    // Chỉ seed khi bảng Clubs còn trống (chạy lại nhiều lần không bị nhân đôi).
    public Task SeedAsync(CancellationToken ct = default)
        => throw new NotImplementedException("TODO: seed dữ liệu mẫu");
}
