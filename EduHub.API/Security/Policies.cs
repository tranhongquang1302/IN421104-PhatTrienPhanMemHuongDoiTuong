using EduHub.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace EduHub.API.Security;

/// <summary>Tên các Authorization Policy và cách định nghĩa chúng.</summary>
public static class Policies
{
    public const string AdminOnly = "AdminOnly";
    public const string CanIssueQr = "CanIssueQr";

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(AdminOnly, p => p.RequireRole(Roles.Admin));
        options.AddPolicy(CanIssueQr, p => p.RequireAuthenticatedUser().RequireAssertion(context =>
            context.User.IsInRole(Roles.Admin) || context.User.HasClaim(AppClaims.Permission, AppClaims.QrIssue)));
    }
}
