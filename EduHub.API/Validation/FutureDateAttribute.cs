using System.ComponentModel.DataAnnotations;

namespace EduHub.API.Validation;

// Validation tùy biến: thời điểm phải ở tương lai. Lỗi trả về 400 ValidationProblemDetails.
[AttributeUsage(AttributeTargets.Property)]
public sealed class FutureDateAttribute : ValidationAttribute
{
    public FutureDateAttribute() : base("Ngày diễn ra phải lớn hơn thời điểm hiện tại.") { }

    public override bool IsValid(object? value)
        => value is DateTime dt && dt.ToUniversalTime() > DateTime.UtcNow;
}
