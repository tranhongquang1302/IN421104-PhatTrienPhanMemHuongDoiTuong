using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

namespace Ch02.Refactor.Tests;

public class PatternTests
{
    [Theory]
    [InlineData(NotificationChannel.Email, "EMAIL")]
    [InlineData(NotificationChannel.Sms, "SMS")]
    [InlineData(NotificationChannel.Slack, "SLACK")]
    public void Factory_WithSupportedChannel_SendsThroughRequestedChannel(
        NotificationChannel channel, string label)
    {
        // Arrange
        using var output = new StringWriter();
        INotificationServiceFactory factory = new NotificationServiceFactory(output);

        // Act
        factory.Create(channel).Send("recipient", "Hello");

        // Assert
        Assert.Equal($"[{label}] to recipient: Hello{output.NewLine}", output.ToString());
    }

    [Fact]
    public void Factory_WithUnknownChannel_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        using var output = new StringWriter();
        INotificationServiceFactory factory = new NotificationServiceFactory(output);

        // Act
        var act = () => factory.Create((NotificationChannel)999);

        // Assert
        Assert.Equal("channel", Assert.Throws<ArgumentOutOfRangeException>(act).ParamName);
        Assert.Equal(string.Empty, output.ToString());
    }

    [Fact]
    public void Decorator_AddThenQuery_PreservesRepositoryResultsAndLogsOperations()
    {
        // Arrange
        var inner = new InMemoryClubRepository();
        var logger = new RecordingLogger();
        IClubRepository repository = new LoggingClubRepository(inner, logger);
        var club = Club.Create("Tech Club", "mô tả");

        // Act
        repository.Add(club);
        var found = repository.GetById(club.Id);
        var all = repository.GetAll();

        // Assert
        Assert.Equal(club.Id, Assert.Single(inner.GetAll()).Id);
        Assert.Equal(club.Id, found?.Id);
        Assert.Equal(club.Id, Assert.Single(all).Id);
        Assert.Collection(logger.Messages,
            message => Assert.Contains("[repo] Add", message),
            message => Assert.Contains("[repo] GetById", message),
            message => Assert.Contains("[repo] GetAll", message));
    }

    [Fact]
    public void Decorator_GetByIdWithUnknownClub_PreservesNullResult()
    {
        // Arrange
        var logger = new RecordingLogger();
        IClubRepository repository = new LoggingClubRepository(new InMemoryClubRepository(), logger);
        var id = Guid.NewGuid();

        // Act
        var club = repository.GetById(id);

        // Assert
        Assert.Null(club);
        Assert.Contains(id.ToString(), Assert.Single(logger.Messages));
    }

    [Theory]
    [InlineData("Tech, Club", "\"Tech, Club\"")]
    [InlineData("Tech \"Club\"", "\"Tech \"\"Club\"\"\"")]
    [InlineData("Tech\r\nClub", "\"Tech\r\nClub\"")]
    public void CsvStrategy_WithSpecialCharacters_QuotesAndEscapesClubName(string name, string expectedName)
    {
        // Arrange
        IReportStrategy strategy = new CsvReportStrategy();
        var stats = new[] { new ClubStat(name, 2, 1, 2, 1) };

        // Act
        var csv = strategy.Render(stats);

        // Assert
        var expected = $"Club,Members,Events,Registered,CheckedIn{Environment.NewLine}{expectedName},2,1,2,1";
        Assert.Equal(expected, csv);
    }

    [Fact]
    public void CsvStrategy_WithNoClubs_ReturnsHeaderOnly()
    {
        // Arrange
        IReportStrategy strategy = new CsvReportStrategy();

        // Act
        var csv = strategy.Render(Array.Empty<ClubStat>());

        // Assert
        Assert.Equal("Club,Members,Events,Registered,CheckedIn", csv);
    }

    [Fact]
    public void ConsoleStrategy_WithStatistics_IncludesAllMetrics()
    {
        // Arrange
        IReportStrategy strategy = new ConsoleReportStrategy();
        var stats = new[] { new ClubStat("Tech Club", 2, 3, 5, 4) };

        // Act
        var report = strategy.Render(stats);

        // Assert
        Assert.Equal("Club: Tech Club | Members: 2 | Events: 3 | Registered: 5 | CheckedIn: 4", report);
    }

    [Fact]
    public void Generate_WithCustomStrategy_AggregatesEachClubAndUsesSelectedStrategy()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var members = new InMemoryMemberRepository();
        var events = new InMemoryEventRepository();
        var techClub = Club.Create("Tech Club", "mô tả");
        var musicClub = Club.Create("Music Club", "mô tả");
        clubs.Add(techClub);
        clubs.Add(musicClub);
        var firstMember = AddMember(members, techClub.Id, "An");
        var secondMember = AddMember(members, techClub.Id, "Bình");
        var musicMember = AddMember(members, musicClub.Id, "Chi");
        var firstEvent = AddEvent(events, techClub.Id, "Workshop");
        var secondEvent = AddEvent(events, techClub.Id, "Meetup");
        var musicEvent = AddEvent(events, musicClub.Id, "Concert");
        firstEvent.RegisteredMemberIds.UnionWith(new[] { firstMember.Id, secondMember.Id });
        firstEvent.CheckedInMemberIds.Add(firstMember.Id);
        secondEvent.RegisteredMemberIds.Add(firstMember.Id);
        musicEvent.RegisteredMemberIds.Add(musicMember.Id);
        musicEvent.CheckedInMemberIds.Add(musicMember.Id);
        IReportService service = new ReportService(clubs, members, events, new RecordingLogger());
        var strategy = new CapturingStrategy();

        // Act
        var report = service.Generate(strategy);

        // Assert
        Assert.Equal("custom report", report);
        Assert.Equal<ClubStat>(new[]
        {
            new ClubStat("Tech Club", 2, 2, 3, 1),
            new ClubStat("Music Club", 1, 1, 1, 1)
        }, strategy.Stats);
    }

    [Fact]
    public void CreateEvent_WithSubscribedObservers_NotifiesEachOnceAfterSaving()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var events = new InMemoryEventRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        IEventService service = new EventService(clubs, events);
        var first = new RecordingObserver
        {
            OnNotification = (ev, owner) =>
                Assert.Equal(ev.Id, Assert.Single(events.GetByClub(owner.Id)).Id)
        };
        var second = new RecordingObserver();
        service.Subscribe(first);
        service.Subscribe(first);
        service.Subscribe(second);

        // Act
        var ev = service.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));

        // Assert
        Assert.Equal(ev.Id, Assert.Single(first.Calls).Event.Id);
        Assert.Equal(ev.Id, Assert.Single(second.Calls).Event.Id);
        Assert.Equal(club.Id, second.Calls[0].Club.Id);
    }

    [Fact]
    public void Unsubscribe_AfterFirstEvent_StopsReceivingLaterEvents()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var events = new InMemoryEventRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        IEventService service = new EventService(clubs, events);
        var observer = new RecordingObserver();
        service.Subscribe(observer);
        var firstEvent = service.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));

        // Act
        var removed = service.Unsubscribe(observer);
        service.CreateEvent(club.Id, "Meetup", new DateTime(2026, 11, 21));

        // Assert
        Assert.True(removed);
        Assert.Equal(firstEvent.Id, Assert.Single(observer.Calls).Event.Id);
    }

    [Fact]
    public void CreateEvent_UnsubscribeDuringCallback_AffectsOnlyLaterEvents()
    {
        // Arrange
        var clubs = new InMemoryClubRepository();
        var events = new InMemoryEventRepository();
        var club = Club.Create("Tech Club", "mô tả");
        clubs.Add(club);
        IEventService service = new EventService(clubs, events);
        var second = new RecordingObserver();
        var first = new RecordingObserver
        {
            OnNotification = (_, _) => { service.Unsubscribe(second); }
        };
        service.Subscribe(first);
        service.Subscribe(second);

        // Act
        var firstEvent = service.CreateEvent(club.Id, "Workshop", new DateTime(2026, 11, 20));
        service.CreateEvent(club.Id, "Meetup", new DateTime(2026, 11, 21));

        // Assert
        Assert.Equal(2, first.Calls.Count);
        Assert.Equal(firstEvent.Id, Assert.Single(second.Calls).Event.Id);
    }

    private static Member AddMember(IMemberRepository repository, Guid clubId, string name)
    {
        var member = new Member
        {
            Id = Guid.NewGuid(),
            ClubId = clubId,
            Name = name,
            Email = $"{Guid.NewGuid():N}@example.com"
        };
        repository.Add(member);
        return member;
    }

    private static Event AddEvent(IEventRepository repository, Guid clubId, string title)
    {
        var ev = new Event
        {
            Id = Guid.NewGuid(),
            ClubId = clubId,
            Title = title,
            Date = new DateTime(2026, 11, 20)
        };
        repository.Add(ev);
        return ev;
    }

    private sealed class RecordingLogger : IAppLogger
    {
        public List<string> Messages { get; } = new();

        public void Info(string message) => Messages.Add(message);
    }

    private sealed class CapturingStrategy : IReportStrategy
    {
        public string Name => "Custom";
        public IReadOnlyList<ClubStat> Stats { get; private set; } = Array.Empty<ClubStat>();

        public string Render(IReadOnlyList<ClubStat> stats)
        {
            Stats = stats;
            return "custom report";
        }
    }

    private sealed class RecordingObserver : IEventObserver
    {
        public List<(Event Event, Club Club)> Calls { get; } = new();
        public Action<Event, Club>? OnNotification { get; init; }

        public void OnEventCreated(Event ev, Club club)
        {
            Calls.Add((ev, club));
            OnNotification?.Invoke(ev, club);
        }
    }
}
