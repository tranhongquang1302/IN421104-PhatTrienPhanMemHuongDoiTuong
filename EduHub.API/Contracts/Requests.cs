using System.ComponentModel.DataAnnotations;
using EduHub.API.Validation;

namespace EduHub.API.Contracts;

// [ApiController] trả ValidationProblemDetails khi request không hợp lệ.
public class CreateEventRequest
{
    [Required(ErrorMessage = "Tên sự kiện không được để trống.")]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    public required Guid ClubId { get; init; }

    [StringLength(200)]
    public string? Location { get; init; }

    [FutureDate]
    public DateTime StartTime { get; init; }

    [Range(1, 10000, ErrorMessage = "Capacity phải nằm trong khoảng từ 1 đến 10000.")]
    public int Capacity { get; init; }
}

public class UpdateEventRequest
{
    [Required(ErrorMessage = "Tên sự kiện không được để trống.")]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [StringLength(200)]
    public string? Location { get; init; }

    [FutureDate]
    public DateTime StartTime { get; init; }

    [Range(1, 10000, ErrorMessage = "Capacity phải nằm trong khoảng từ 1 đến 10000.")]
    public int Capacity { get; init; }
}

public class SaveClubRequest
{
    [Required(ErrorMessage = "Tên câu lạc bộ không được để trống.")]
    [StringLength(150)]
    public string Name { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; init; }
}

public class RegisterRequest
{
    public required Guid MemberId { get; init; }
}
