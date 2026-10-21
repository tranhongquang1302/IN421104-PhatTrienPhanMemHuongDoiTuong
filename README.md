# EduHub Ch02 — Refactoring với SOLID và Design Patterns

**Repository môn học:** [IN421104-PhatTrienPhanMemHuongDoiTuong](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong).

Dự án đã hoàn thành phân tích SOLID, refactor theo trách nhiệm, áp dụng Factory, Decorator, Strategy, Observer và hoàn thiện nghiệp vụ Domain cùng các unit test Arrange–Act–Assert. Các service nhận phụ thuộc qua interface; `Program.cs` ráp các thành phần và minh họa cả bốn pattern.

## Cấu trúc sau refactor

| Thành phần | Trách nhiệm |
|---|---|
| `Ch02.Refactor/Domain/` | `Club` kiểm tra tên, sức chứa; `Event` quản lý đăng ký và check-in theo quy tắc nghiệp vụ. |
| `Ch02.Refactor/Services/` | `ClubService`, `EventService`, `ReportService` cùng các interface điều phối từng nghiệp vụ. |
| `Ch02.Refactor/Repositories/` | Các hợp đồng lưu trữ CLB, thành viên, sự kiện và bản cài đặt trong bộ nhớ. Kết quả truy vấn là snapshot. |
| `Ch02.Refactor/Logging/` | `IAppLogger`, `ConsoleLogger`, `FileLogger`; demo hiển thị log trên Console. |
| `Ch02.Refactor/Notifications/` | Factory tạo Email, SMS, Slack; thông báo được mô phỏng qua `TextWriter`. |
| `Ch02.Refactor/Patterns/Strategies/` | Các chiến lược xuất thống kê thành Console hoặc CSV. |
| `Ch02.Refactor/Patterns/Observers/` | Các observer gửi email và ghi log khi tạo sự kiện. |
| `Ch02.Refactor/Program.cs` | Composition root và demo tạo CLB, thêm thành viên, tạo sự kiện, xuất báo cáo. |
| `Ch02.Refactor.Tests/ServiceTests.cs` | Test nghiệp vụ với repository trong bộ nhớ, logger và notifier giả. |
| `Ch02.Refactor.Tests/PatternTests.cs` | Test cho cả bốn pattern, gồm CSV có ký tự đặc biệt và vòng đời đăng ký observer. |
| `Ch02.Refactor.Tests/Domain/ClubTests.cs` | Test tên hợp lệ/không hợp lệ, ID và các biên sức chứa của CLB. |
| `Ch02.Refactor.Tests/Domain/EventTests.cs` | Test đăng ký, chống trùng và điều kiện check-in của từng sự kiện. |

Tên CLB bắt buộc và tối đa 100 ký tự; mỗi CLB tối đa 50 thành viên. Thêm thành viên hoặc tạo sự kiện với CLB không tồn tại phải báo lỗi. Khi thêm thành viên, các kênh thông báo được chọn ở composition root; khi tạo sự kiện, chỉ thành viên thuộc CLB đó nhận tin.

`Member.ClubId` chỉ được gán lúc khởi tạo bằng `init`. Code gọi không thể đổi CLB trực tiếp trên thành viên đã được service trả về để bỏ qua kiểm tra giới hạn 50 người.

`Event.Register` trả về `true` khi đăng ký mới và `false` khi trùng thành viên. `Event.CheckIn` chỉ chấp nhận thành viên đã đăng ký chính sự kiện đó; trường hợp khác ném `InvalidOperationException`. Check-in lặp lại giữ một lượt tham dự. Các tập ID nằm trong field riêng và chỉ được đọc qua snapshot, để code gọi sử dụng `Register`/`CheckIn` khi thay đổi trạng thái.

## Bốn Design Pattern

| Pattern | Thành phần | Cách sử dụng |
|---|---|---|
| Factory | [NotificationServiceFactory](Ch02.Refactor/Notifications/NotificationServiceFactory.cs) | `Create(NotificationChannel)` trả về `INotificationService` cho Email, SMS hoặc Slack. Kênh không hợp lệ gây `ArgumentOutOfRangeException`. |
| Decorator | [LoggingClubRepository](Ch02.Refactor/Repositories/LoggingClubRepository.cs) | Bọc `IClubRepository`, ghi log `Add`, `GetById`, `GetAll`, rồi chuyển thao tác cho repository bên trong. |
| Strategy | [IReportStrategy](Ch02.Refactor/Patterns/Strategies/IReportStrategy.cs) | `ReportService.Generate(strategy)` tạo cùng bộ số liệu và giao định dạng cho `ConsoleReportStrategy` hoặc `CsvReportStrategy`. |
| Observer | [IEventObserver](Ch02.Refactor/Patterns/Observers/IEventObserver.cs) | `EventService` lưu sự kiện rồi báo cho các observer đăng ký qua `Subscribe`; `EmailNotifier` gửi tin, `LogNotifier` ghi log. Dùng `Unsubscribe` để ngừng nhận tin. |

`ReportService` thống kê tên CLB, số thành viên, số sự kiện, tổng lượt đăng ký và tổng lượt check-in. Một thành viên đăng ký hai sự kiện được tính là hai lượt đăng ký. CSV đặt tên CLB trong ngoặc kép và escape dấu ngoặc kép bên trong tên.

Một observer được đăng ký nhiều lần vẫn chỉ nhận một thông báo cho mỗi sự kiện. Thay đổi đăng ký trong callback có hiệu lực từ sự kiện sau, vì lần phát hiện tại duyệt snapshot các observer.

Demo dùng `ConsoleLogger` để hiển thị cả log truy cập repository và log sự kiện. Có thể đổi sang `FileLogger("club-log.txt")` tại composition root. Các kênh thông báo dùng `Console.Out` trong demo và `StringWriter` trong test.

## Lệnh

```bash
dotnet build Ch02.Refactor.sln
dotnet test Ch02.Refactor.sln
dotnet run --project Ch02.Refactor
```

Demo có bốn phần: Decorator khi tạo CLB, Observer khi tạo sự kiện, Strategy với hai định dạng báo cáo và Factory với đủ ba kênh thông báo. Hai thành viên được đăng ký sự kiện và một người check-in; cả báo cáo Console lẫn CSV hiển thị số liệu `Members=2`, `Events=1`, `Registered=2`, `CheckedIn=1`.

Các test service kiểm tra giới hạn 49/50 thành viên, lỗi dữ liệu đầu vào, CLB không tồn tại, phạm vi người nhận thông báo, số liệu báo cáo và snapshot truy vấn. Các test pattern kiểm tra chọn kênh, hợp đồng repository, định dạng CSV, thay strategy và đăng ký/hủy observer.

Các test Domain chia theo `ClubTests` và `EventTests`, mỗi test có ba phần Arrange, Act, Assert. Các trường hợp kiểm tra gồm tên null/rỗng/toàn khoảng trắng, tên dài 100/101 ký tự, sức chứa 0/49/50/51, đăng ký trùng và check-in chưa đăng ký hoặc chỉ đăng ký sự kiện khác. Toàn bộ test được thực thi, không còn placeholder `Skip`.

## Tiến độ bài tập

- [x] Nhiệm vụ 1: [phân tích SOLID](docs/solid-analysis.md), gồm 6 vấn đề với vị trí mã, hậu quả và đề xuất sửa.
- [x] Nhiệm vụ 2: tách class theo trách nhiệm và truyền phụ thuộc qua interface, constructor.
- [x] Nhiệm vụ 3: Factory, Decorator, Strategy, Observer; có demo và test cho cả bốn pattern.
- [x] Nhiệm vụ 4: hoàn thiện `Event.Register`, `Event.CheckIn` và các test Domain theo Arrange–Act–Assert, gồm cả trường hợp lỗi và giá trị biên.
- [x] Nhiệm vụ 5: đủ ít nhất 5 commit có ý nghĩa, đã rebase lên `main`; [Pull Request #3](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/pull/3) có mô tả What, Why, How to test và hai nhận xét tự review.

## Nộp bài

Làm trên nhánh `ch02_0024419387_feature/solid-refactor`, commit theo Conventional Commits, rebase lên `main` rồi mở Pull Request với mô tả What, Why, How to test. Nộp link Pull Request theo hướng dẫn trên LMS.
