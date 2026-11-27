namespace EduHub.Domain.Entities;

/// <summary>Tên vai trò (role). Giá trị này được lưu trong Identity và đi vào claim "role" của JWT.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Member = "Member";
}

/// <summary>Claim bổ sung gắn cho người dùng (ngoài vai trò), dùng cho Authorization Policy.</summary>
public static class AppClaims
{
    public const string Permission = "permission";
    public const string QrIssue = "qr.issue";   // được phép tạo mã QR điểm danh
}
