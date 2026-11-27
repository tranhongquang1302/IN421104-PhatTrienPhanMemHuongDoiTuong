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
        // Policy 1: chỉ Admin
        options.AddPolicy(AdminOnly, p => p.RequireRole(Roles.Admin));

        // Policy 2: Admin, HOẶC người có claim permission = qr.issue (vd: ban tổ chức)
        options.AddPolicy(CanIssueQr, p => p.RequireAssertion(ctx =>
            ctx.User.IsInRole(Roles.Admin) ||
            ctx.User.HasClaim(AppClaims.Permission, AppClaims.QrIssue)));
    }
}
