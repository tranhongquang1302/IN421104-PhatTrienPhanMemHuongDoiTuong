using System.ComponentModel.DataAnnotations;
using EduHub.API.Validation;

namespace EduHub.API.Contracts;

// Request DTO. [ApiController] tự trả 400 (ValidationProblemDetails) khi dữ liệu không hợp lệ.
public class CreateEventRequest
{
    // TODO [Nhiệm vụ 3] Thêm DataAnnotations:
    //  Name: [Required(ErrorMessage = "Tên sự kiện không được để trống.")] và [StringLength(200)]
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [Required]
    public Guid ClubId { get; init; }

    [StringLength(200)]
    public string? Location { get; init; }

    // TODO [Nhiệm vụ 3] Thêm [FutureDate] (đã có sẵn trong EduHub.API/Validation)
    public DateTime StartTime { get; init; }

    // TODO [Nhiệm vụ 3] Thêm [Range(1, 10000, ErrorMessage = "Capacity phải lớn hơn 0.")]
    public int Capacity { get; init; }
}

public class UpdateEventRequest
{
    // TODO [Nhiệm vụ 3] Áp dụng cùng các quy tắc validation như CreateEventRequest (Name, StartTime, Capacity)
    public string Name { get; init; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; init; }

    [StringLength(200)]
    public string? Location { get; init; }

    public DateTime StartTime { get; init; }

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
    [Required]
    public Guid MemberId { get; init; }
}
