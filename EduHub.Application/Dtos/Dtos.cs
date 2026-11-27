using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;

namespace EduHub.Application.Dtos;

// DTO (Data Transfer Object): hình dạng dữ liệu trả ra ngoài, tách khỏi entity.
public record ClubDto(Guid Id, string Name, string Description, DateTime CreatedAt);

public record EventDto(Guid Id, string Name, string Description, Guid ClubId, string Location,
    DateTime StartTime, int Capacity, DateTime CreatedAt);

public record RegistrationDto(Guid Id, Guid EventId, Guid MemberId, string Status, DateTime RegisteredAt);

public record RegistrationMemberDto(Guid RegistrationId, Guid MemberId, string StudentId, string FullName,
    string Email, string Status, DateTime RegisteredAt);

// Mapping viết tay: ngắn, dễ debug, không cần thư viện (AutoMapper/Mapperly là lựa chọn khi dự án lớn)
public static class MappingExtensions
{
    public static ClubDto ToDto(this Club c) => new(c.Id, c.Name, c.Description, c.CreatedAt);

    public static EventDto ToDto(this Event e) =>
        new(e.Id, e.Name, e.Description, e.ClubId, e.Location, e.StartTime, e.Capacity, e.CreatedAt);

    public static RegistrationDto ToDto(this Registration r) =>
        new(r.Id, r.EventId, r.MemberId, r.Status, r.RegisteredAt);

    public static RegistrationMemberDto ToDto(this RegistrationWithMember x) =>
        new(x.Registration.Id, x.Member.Id, x.Member.StudentId, x.Member.FullName, x.Member.Email,
            x.Registration.Status, x.Registration.RegisteredAt);
}
