using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

public class ServiceTests
{
    [Fact]
    public void CreateClub_WithValidName_PersistsClubAndNotifiesPresident()
    {
        // Arrange
        var context = new TestContext();
        const string presidentEmail = "president@gmail.com";

        // Act
        var club = context.ClubService.CreateClub("Tech Club", "mô tả", presidentEmail);

        // Assert
        Assert.NotEqual(Guid.Empty, club.Id);
        Assert.Equal(club.Id, Assert.Single(context.Clubs.GetAll()).Id);
        Assert.Equal(presidentEmail, Assert.Single(context.Email.Messages).To);
        Assert.Empty(context.Slack.Messages);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CreateClub_WithInvalidName_DoesNotSaveOrNotify(int nameLength)
    {
        // Arrange
        var context = new TestContext();
        var name = new string('a', nameLength);

        // Act
        var act = () => context.ClubService.CreateClub(name, "mô tả", "p@gmail.com");

        // Assert
        Assert.Throws<ArgumentException>(act);
        Assert.Empty(context.Clubs.GetAll());
        Assert.Empty(context.Email.Messages);
        Assert.Empty(context.Logger.Messages);
    }

    [Fact]
    public void AddMemberToClub_WithConfiguredChannels_NotifiesEachChannel()
    {
        // Arrange
        var context = new TestContext();
        var club = context.AddClub("Tech Club");

        // Act
        var member = context.ClubService.AddMemberToClub(club.Id, "An", "an@example.com");

        // Assert
        Assert.Equal(member.Id, Assert.Single(context.Members.GetByClub(club.Id)).Id);
        Assert.Equal(member.Email, Assert.Single(context.Email.Messages).To);
        Assert.Equal(member.Email, Assert.Single(context.Slack.Messages).To);
    }

    [Fact]
    public void AddMemberToClub_With49Members_AcceptsLastSlotRegardlessOfOtherClubs()
    {
        // Arrange
        var context = new TestContext();
        var club = context.AddClub("Tech Club");
        var otherClub = context.AddClub("Music Club");
        context.AddMembers(club.Id, 49);
        context.AddMembers(otherClub.Id, 1);

        // Act
        var member = context.ClubService.AddMemberToClub(club.Id, "An", "an@example.com");

        // Assert
        var members = context.Members.GetByClub(club.Id);
        Assert.Equal(50, members.Count);
        Assert.Contains(members, existing => existing.Id == member.Id);
    }

    [Fact]
    public void AddMemberToClub_With50Members_RejectsWithoutSavingOrNotifying()
    {
        // Arrange
        var context = new TestContext();
        var club = context.AddClub("Tech Club");
        context.AddMembers(club.Id, 50);

        // Act
        var act = () => context.ClubService.AddMemberToClub(club.Id, "An", "an@example.com");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Equal(50, context.Members.GetByClub(club.Id).Count);
        Assert.Empty(context.Email.Messages);
        Assert.Empty(context.Slack.Messages);
        Assert.Empty(context.Logger.Messages);
    }

    [Fact]
    public void AddMemberToClub_WithUnknownClub_RejectsWithoutSavingOrNotifying()
    {
        // Arrange
        var context = new TestContext();
        var clubId = Guid.NewGuid();

        // Act
        var act = () => context.ClubService.AddMemberToClub(clubId, "An", "an@example.com");

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Empty(context.Members.GetByClub(clubId));
        Assert.Empty(context.Email.Messages);
        Assert.Empty(context.Slack.Messages);
    }

    [Fact]
    public void CreateEvent_WithMembersInDifferentClubs_NotifiesOnlySelectedClub()
    {
        // Arrange
        var context = new TestContext();
        var club = context.AddClub("Tech Club");
        var otherClub = context.AddClub("Music Club");
        context.AddMembers(club.Id, 2);
        context.AddMembers(otherClub.Id, 1);
        var expectedRecipients = context.Members.GetByClub(club.Id)
            .Select(member => member.Email).ToArray();

        // Act
        var ev = context.EventService.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));

        // Assert
        Assert.Equal(ev.Id, Assert.Single(context.Events.GetByClub(club.Id)).Id);
        Assert.Empty(context.Events.GetByClub(otherClub.Id));
        Assert.Equal(expectedRecipients, context.Email.Messages.Select(message => message.To));
    }

    [Fact]
    public void CreateEvent_WithUnknownClub_RejectsWithoutSavingOrNotifying()
    {
        // Arrange
        var context = new TestContext();
        var clubId = Guid.NewGuid();

        // Act
        var act = () => context.EventService.CreateEvent(clubId, "Workshop", new DateTime(2026, 11, 20));

        // Assert
        Assert.Throws<InvalidOperationException>(act);
        Assert.Empty(context.Events.GetByClub(clubId));
        Assert.Empty(context.Email.Messages);
        Assert.Empty(context.Logger.Messages);
    }

    [Fact]
    public void Generate_WithMultipleClubs_CountsMembersPerClub()
    {
        // Arrange
        var context = new TestContext();
        var techClub = context.AddClub("Tech Club");
        var musicClub = context.AddClub("Music Club");
        context.AddMembers(techClub.Id, 2);
        context.AddMembers(musicClub.Id, 1);

        // Act
        var report = context.ReportService.Generate(new ConsoleReportStrategy());

        // Assert
        var expected = "Club: Tech Club | Members: 2 | Events: 0 | Registered: 0 | CheckedIn: 0" +
            Environment.NewLine +
            "Club: Music Club | Members: 1 | Events: 0 | Registered: 0 | CheckedIn: 0";
        Assert.Equal(expected, report);
    }

    [Fact]
    public void GetAllClubs_AfterAddingAnotherClub_PreviousSnapshotStaysUnchanged()
    {
        // Arrange
        var context = new TestContext();
        var firstClub = context.AddClub("Tech Club");
        var snapshot = context.ClubService.GetAllClubs();

        // Act
        context.AddClub("Music Club");

        // Assert
        Assert.Equal(firstClub.Id, Assert.Single(snapshot).Id);
        Assert.Equal(2, context.ClubService.GetAllClubs().Count);
    }

    private sealed class TestContext
    {
        public InMemoryClubRepository Clubs { get; } = new();
        public InMemoryMemberRepository Members { get; } = new();
        public InMemoryEventRepository Events { get; } = new();
        public RecordingLogger Logger { get; } = new();
        public RecordingNotificationService Email { get; } = new();
        public RecordingNotificationService Slack { get; } = new();
        public IClubService ClubService { get; }
        public IEventService EventService { get; }
        public IReportService ReportService { get; }

        public TestContext()
        {
            ClubService = new ClubService(Clubs, Members, Logger, Email, new[] { Email, Slack });
            EventService = new EventService(Clubs, Events);
            EventService.Subscribe(new LogNotifier(Logger));
            EventService.Subscribe(new EmailNotifier(Members, Email));
            ReportService = new ReportService(Clubs, Members, Events, Logger);
        }

        public Club AddClub(string name)
        {
            var club = Club.Create(name, "mô tả");
            Clubs.Add(club);
            return club;
        }

        public void AddMembers(Guid clubId, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Members.Add(new Member
                {
                    Id = Guid.NewGuid(),
                    ClubId = clubId,
                    Name = $"Member {i}",
                    Email = $"member-{i}-{clubId:N}@example.com"
                });
            }
        }
    }

    private sealed class RecordingLogger : IAppLogger
    {
        public List<string> Messages { get; } = new();

        public void Info(string message) => Messages.Add(message);
    }

    private sealed class RecordingNotificationService : INotificationService
    {
        public List<(string To, string Message)> Messages { get; } = new();

        public void Send(string to, string message) => Messages.Add((to, message));
    }
}
