using EduHub.Ch02.Logging;
using EduHub.Ch02.Notifications;
using EduHub.Ch02.Repositories;
using EduHub.Ch02.Services;

// Composition root: tạo các bản cài đặt và truyền phụ thuộc qua constructor.
IAppLogger logger = new FileLogger("club-log.txt");

// TODO (Nhiệm vụ 3): tạo các kênh thông báo bằng Factory.
INotificationService emailNotifier = new EmailNotificationService();
INotificationService slackNotifier = new SlackNotificationService();

// TODO (Nhiệm vụ 3): bọc clubRepository bằng LoggingClubRepository (Decorator).
IClubRepository clubRepository = new InMemoryClubRepository();
IMemberRepository memberRepository = new InMemoryMemberRepository();
IEventRepository eventRepository = new InMemoryEventRepository();

IClubService clubService = new ClubService(
    clubRepository, memberRepository, logger, emailNotifier,
    new[] { emailNotifier, slackNotifier });
IEventService eventService = new EventService(
    clubRepository, memberRepository, eventRepository, logger, emailNotifier);
IReportService reportService = new ReportService(clubRepository, memberRepository, logger);

var club = clubService.CreateClub("Tech Club", "Câu lạc bộ công nghệ", "president@gmail.com");
clubService.AddMemberToClub(club.Id, "Nguyễn Văn A", "a@student.dthu.edu.vn");
eventService.CreateEvent(club.Id, "Workshop SOLID", new DateTime(2026, 11, 20));
Console.WriteLine(reportService.Generate());
