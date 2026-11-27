namespace EduHub.Application.Common;

// Lỗi nghiệp vụ: handler chỉ việc throw, GlobalExceptionHandler ở lớp API đổi thành ProblemDetails.
public abstract class AppException(string message) : Exception(message);

public class NotFoundException(string message) : AppException(message);          // 404

public class ConflictException(string message) : AppException(message);          // 409

public class BusinessRuleException(string message) : AppException(message);      // 400
