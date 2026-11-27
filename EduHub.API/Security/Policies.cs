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
        // TODO 3.4: policy AdminOnly = chỉ người có role Admin (p.RequireRole)
        // TODO 3.5: policy CanIssueQr = role Admin HOẶC có claim permission = qr.issue (p.RequireAssertion)
    }
}
