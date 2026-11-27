namespace EduHub.Tests.Clubs;

public class CreateClubCommandTests
{
    // TODO: CreateClub_WithValidName_SavesAndReturnsClub
    //       - mock IClubRepository, gọi CreateClubCommandHandler
    //       - Assert tên CLB, và Verify AddAsync được gọi đúng 1 lần
    [Fact(Skip = "TODO: viết test")]
    public void CreateClub_WithValidName_SavesAndReturnsClub() { }

    // TODO: CreateClub_WhenNameEmpty_ThrowsValidationException
    //       - dùng [Theory] với "" và "   "
    //       - Verify AddAsync KHÔNG được gọi (Times.Never)
    [Fact(Skip = "TODO: viết test")]
    public void CreateClub_WhenNameEmpty_ThrowsValidationException() { }
}
