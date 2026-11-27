using EduHub.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Middleware;

// Xử lý lỗi toàn cục: mọi exception chưa bắt đều đi qua đây và trở thành ProblemDetails (RFC 9457).
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        // TODO [Nhiệm vụ 6]
        //  1. Đổi exception sang mã trạng thái: NotFoundException -> 404, ConflictException -> 409,
        //     BusinessRuleException -> 400, còn lại -> 500 (switch expression).
        //  2. Ghi log: lỗi 500 dùng LogError(ex, ...) kèm stack trace; lỗi nghiệp vụ chỉ cần LogWarning.
        //  3. Dựng ProblemDetails { Status, Title, Detail, Instance = ctx.Request.Path } và thêm Extensions["traceId"].
        //     Lỗi 500 ngoài môi trường Development: không trả ex.Message ra ngoài.
        //  4. ctx.Response.StatusCode = status; ghi JSON với content type "application/problem+json"; return true.
        throw new NotImplementedException();
    }
}
