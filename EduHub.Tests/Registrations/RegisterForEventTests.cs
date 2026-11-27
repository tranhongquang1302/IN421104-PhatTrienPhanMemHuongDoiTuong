namespace EduHub.Tests.Registrations;

// TODO [Nhiệm vụ 4] Viết các test cho RegisterForEventCommandHandler bằng Moq
// (mock IEventRepository, IRegistrationRepository, IUnitOfWork; ILogger dùng NullLogger<T>.Instance).
// Bỏ thuộc tính Skip khi bắt đầu viết test.
public class RegisterForEventTests
{
    [Fact(Skip = "TODO: đăng ký hợp lệ -> Add được gọi một lần, SaveChangesAsync được gọi một lần")]
    public Task Register_Valid_AddsAndSavesOnce() => Task.CompletedTask;

    [Fact(Skip = "TODO: sự kiện không tồn tại -> NotFoundException")]
    public Task Register_UnknownEvent_ThrowsNotFound() => Task.CompletedTask;

    [Fact(Skip = "TODO: đã đăng ký -> ConflictException")]
    public Task Register_Duplicate_ThrowsConflict() => Task.CompletedTask;

    [Fact(Skip = "TODO: sự kiện đầy -> BusinessRuleException và không gọi SaveChangesAsync")]
    public Task Register_WhenFull_ThrowsBusinessRule() => Task.CompletedTask;
}
