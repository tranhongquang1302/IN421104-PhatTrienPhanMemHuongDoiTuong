using EduHub.Ch02.Domain;

namespace Ch02.Refactor.Tests.Domain;

public class ClubTests
{
    [Fact]
    public void Create_WithValidName_ReturnsClubWithNameDescriptionAndNewId()
    {
        // Arrange
        const string name = "Tech Club";
        const string description = "Câu lạc bộ công nghệ";

        // Act
        var club = Club.Create(name, description);

        // Assert
        Assert.Equal(name, club.Name);
        Assert.Equal(description, club.Description);
        Assert.NotEqual(Guid.Empty, club.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void Create_WithBlankName_ThrowsArgumentException(string? name)
    {
        // Arrange
        const string description = "mô tả";

        // Act
        var act = () => Club.Create(name!, description);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_With100CharacterName_ReturnsClub()
    {
        // Arrange
        var name = new string('a', 100);

        // Act
        var club = Club.Create(name, "mô tả");

        // Assert
        Assert.Equal(name, club.Name);
    }

    [Fact]
    public void Create_With101CharacterName_ThrowsArgumentException()
    {
        // Arrange
        var name = new string('a', 101);

        // Act
        var act = () => Club.Create(name, "mô tả");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(49, true)]
    [InlineData(50, false)]
    [InlineData(51, false)]
    public void HasRoom_DependsOnCurrentMemberCount(int count, bool expected)
    {
        // Arrange
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        var hasRoom = club.HasRoom(count);

        // Assert
        Assert.Equal(expected, hasRoom);
    }

    [Fact]
    public void Create_WithSameName_CreatesDistinctIds()
    {
        // Arrange
        var firstClub = Club.Create("Tech Club", "mô tả");

        // Act
        var secondClub = Club.Create("Tech Club", "mô tả");

        // Assert
        Assert.NotEqual(firstClub.Id, secondClub.Id);
    }
}
