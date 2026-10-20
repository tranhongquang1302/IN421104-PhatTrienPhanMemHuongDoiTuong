# EduHub Ch02 — Refactoring với SOLID và Design Patterns

**Repository môn học:** [IN421104-PhatTrienPhanMemHuongDoiTuong](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong).

Dự án đã hoàn thành phân tích SOLID, tách `ClubManager` thành các thành phần theo trách nhiệm và áp dụng Factory, Decorator, Strategy, Observer. Các service nhận phụ thuộc qua interface; `Program.cs` ráp các thành phần và minh họa cả bốn pattern.

## Cấu trúc sau refactor

| Thành phần | Trách nhiệm |
|---|---|
| `Ch02.Refactor/Domain/` | Entity; `Club.Create` kiểm tra tên và `Club.HasRoom` kiểm tra sức chứa. |
| `Ch02.Refactor/Services/` | `ClubService`, `EventService`, `ReportService` cùng các interface điều phối từng nghiệp vụ. |
| `Ch02.Refactor/Repositories/` | Các hợp đồng lưu trữ CLB, thành viên, sự kiện và bản cài đặt trong bộ nhớ. Kết quả truy vấn là snapshot. |
| `Ch02.Refactor/Logging/` | `IAppLogger`, `ConsoleLogger`, `FileLogger`; demo hiển thị log trên Console. |
| `Ch02.Refactor/Notifications/` | Factory tạo Email, SMS, Slack; thông báo được mô phỏng qua `TextWriter`. |
| `Ch02.Refactor/Patterns/Strategies/` | Các chiến lược xuất thống kê thành Console hoặc CSV. |
| `Ch02.Refactor/Patterns/Observers/` | Các observer gửi email và ghi log khi tạo sự kiện. |
| `Ch02.Refactor/Program.cs` | Composition root và demo tạo CLB, thêm thành viên, tạo sự kiện, xuất báo cáo. |
| `Ch02.Refactor.Tests/ServiceTests.cs` | Test nghiệp vụ với repository trong bộ nhớ, logger và notifier giả. |
| `Ch02.Refactor.Tests/PatternTests.cs` | Test cho cả bốn pattern, gồm CSV có ký tự đặc biệt và vòng đời đăng ký observer. |

Tên CLB bắt buộc và tối đa 100 ký tự; mỗi CLB tối đa 50 thành viên. Thêm thành viên hoặc tạo sự kiện với CLB không tồn tại phải báo lỗi. Khi thêm thành viên, các kênh thông báo được chọn ở composition root; khi tạo sự kiện, chỉ thành viên thuộc CLB đó nhận tin.

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

Demo có bốn phần: Decorator khi tạo CLB, Observer khi tạo sự kiện, Strategy với hai định dạng báo cáo và Factory với đủ ba kênh thông báo.

Các test service kiểm tra giới hạn 49/50 thành viên, lỗi dữ liệu đầu vào, CLB không tồn tại, phạm vi người nhận thông báo, số liệu báo cáo và snapshot truy vấn. Các test pattern kiểm tra chọn kênh, hợp đồng repository, định dạng CSV, thay strategy và đăng ký/hủy observer. Sáu test Domain có `Skip` vẫn là các bài cần hoàn thiện ở nhiệm vụ 4.

## Tiến độ bài tập

- [x] Nhiệm vụ 1: [phân tích SOLID](docs/solid-analysis.md), gồm 6 vấn đề với vị trí mã, hậu quả và đề xuất sửa.
- [x] Nhiệm vụ 2: tách class theo trách nhiệm và truyền phụ thuộc qua interface, constructor.
- [x] Nhiệm vụ 3: Factory, Decorator, Strategy, Observer; có demo và test cho cả bốn pattern.
- [ ] Nhiệm vụ 4: hoàn thiện `Event.Register`, `Event.CheckIn` và các test Domain theo Arrange–Act–Assert.
- [ ] Nhiệm vụ 5: đủ ít nhất 5 commit có ý nghĩa, rebase lên `main`, mở Pull Request và có ít nhất 2 nhận xét tự review.

## Nộp bài

Làm trên nhánh `ch02_0024419387_feature/solid-refactor`, commit theo Conventional Commits, rebase lên `main` rồi mở Pull Request với mô tả What, Why, How to test. Nộp link Pull Request theo hướng dẫn trên LMS.
