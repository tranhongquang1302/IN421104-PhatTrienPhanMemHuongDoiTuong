using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

// Composition root: tạo các bản cài đặt và truyền phụ thuộc qua constructor.
IAppLogger logger = new ConsoleLogger();

INotificationServiceFactory notificationFactory = new NotificationServiceFactory(Console.Out);
INotificationService emailNotifier = notificationFactory.Create(NotificationChannel.Email);
INotificationService slackNotifier = notificationFactory.Create(NotificationChannel.Slack);

IClubRepository clubRepository = new LoggingClubRepository(new InMemoryClubRepository(), logger);
IMemberRepository memberRepository = new InMemoryMemberRepository();
IEventRepository eventRepository = new InMemoryEventRepository();

IClubService clubService = new ClubService(
    clubRepository, memberRepository, logger, emailNotifier,
    new[] { emailNotifier, slackNotifier });
IEventService eventService = new EventService(clubRepository, eventRepository);
eventService.Subscribe(new LogNotifier(logger));
eventService.Subscribe(new EmailNotifier(memberRepository, emailNotifier));
IReportService reportService = new ReportService(
    clubRepository, memberRepository, eventRepository, logger);

Console.WriteLine("== 1. Decorator: tạo CLB và thành viên ==");
var club = clubService.CreateClub("Tech Club", "Câu lạc bộ công nghệ", "president@gmail.com");
clubService.AddMemberToClub(club.Id, "Nguyễn Văn A", "a@student.dthu.edu.vn");
clubService.AddMemberToClub(club.Id, "Trần Văn B", "b@student.dthu.edu.vn");

Console.WriteLine("\n== 2. Observer: tạo sự kiện, ghi log và gửi email ==");
eventService.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));

Console.WriteLine("\n== 3. Strategy: xuất cùng số liệu theo Console và CSV ==");
foreach (IReportStrategy strategy in new IReportStrategy[]
         { new ConsoleReportStrategy(), new CsvReportStrategy() })
{
    Console.WriteLine($"-- {strategy.Name} --");
    Console.WriteLine(reportService.Generate(strategy));
}

Console.WriteLine("\n== 4. Factory: tạo đủ ba kênh thông báo ==");
foreach (var channel in Enum.GetValues<NotificationChannel>())
    notificationFactory.Create(channel).Send("demo@example.com", $"Thử kênh {channel}");
