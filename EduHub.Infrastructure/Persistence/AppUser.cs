using Microsoft.AspNetCore.Identity;

namespace EduHub.Infrastructure.Persistence;

/// <summary>Người dùng của EduHub — kế thừa IdentityUser để có Email, PasswordHash, Roles, Claims.</summary>
public class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}

public class AppRole : IdentityRole<Guid>
{
    public AppRole() { }
    public AppRole(string name) : base(name) { }
}
