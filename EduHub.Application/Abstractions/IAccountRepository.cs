namespace EduHub.Application.Abstractions;

/// <summary>Một claim của người dùng (vd: permission = qr.issue).</summary>
public record UserClaim(string Type, string Value);

/// <summary>Thông tin tài khoản mà tầng Application cần. Không phụ thuộc ASP.NET Core Identity.</summary>
public record Account(
    Guid Id,
    string Email,
    string DisplayName,
    string PasswordHash,
    IReadOnlyList<string> Roles,
    IReadOnlyList<UserClaim> Claims);

public record RegisterResult(Account? Account, IReadOnlyList<string> Errors)
{
    public bool Succeeded => Account is not null;
}

/// <summary>Cổng vào kho người dùng (cài đặt bằng ASP.NET Core Identity ở Infrastructure).</summary>
public interface IAccountRepository
{
    Task<Account?> FindByEmailAsync(string email, CancellationToken ct = default);
    Task<Account?> FindByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Tạo người dùng mới với vai trò Member; Identity kiểm tra độ mạnh mật khẩu.</summary>
    Task<RegisterResult> CreateMemberAsync(string email, string displayName, string password, CancellationToken ct = default);
}
