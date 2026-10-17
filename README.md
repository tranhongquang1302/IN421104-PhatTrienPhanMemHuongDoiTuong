# EduHub Ch02 — Refactoring với SOLID và Design Patterns

Dự án đã hoàn thành phân tích SOLID và tách `ClubManager` thành các service, repository, logger và dịch vụ thông báo. Các phụ thuộc được nhận qua interface trong constructor và được ráp tại `Program.cs`.

## Cấu trúc sau refactor

| Thành phần | Trách nhiệm |
|---|---|
| `Ch02.Refactor/Domain/` | Entity; `Club.Create` kiểm tra tên và `Club.HasRoom` kiểm tra sức chứa. |
| `Ch02.Refactor/Services/` | `ClubService`, `EventService`, `ReportService` cùng các interface điều phối từng nghiệp vụ. |
| `Ch02.Refactor/Repositories/` | Các hợp đồng lưu trữ CLB, thành viên, sự kiện và bản cài đặt trong bộ nhớ. Kết quả truy vấn là snapshot. |
| `Ch02.Refactor/Logging/` | `IAppLogger`, `FileLogger`; ghi vào `club-log.txt`. |
| `Ch02.Refactor/Notifications/` | `INotificationService`; Email và Slack được mô phỏng trên Console. |
| `Ch02.Refactor/Program.cs` | Composition root và demo tạo CLB, thêm thành viên, tạo sự kiện, xuất báo cáo. |
| `Ch02.Refactor.Tests/ServiceTests.cs` | Test nghiệp vụ với repository trong bộ nhớ, logger và notifier giả. |

Tên CLB bắt buộc và tối đa 100 ký tự; mỗi CLB tối đa 50 thành viên. Thêm thành viên hoặc tạo sự kiện với CLB không tồn tại phải báo lỗi. Khi thêm thành viên, các kênh thông báo được chọn ở composition root; khi tạo sự kiện, chỉ thành viên thuộc CLB đó nhận tin.

## Lệnh

```bash
dotnet build Ch02.Refactor.sln
dotnet test Ch02.Refactor.sln
dotnet run --project Ch02.Refactor
```

Các test service kiểm tra giới hạn 49/50 thành viên, lỗi dữ liệu đầu vào, CLB không tồn tại, phạm vi người nhận thông báo, số liệu báo cáo và snapshot truy vấn. Sáu test Domain có `Skip` vẫn là các bài cần hoàn thiện ở nhiệm vụ 4.

## Tiến độ bài tập

- [x] Nhiệm vụ 1: [phân tích SOLID](docs/solid-analysis.md), gồm 6 vấn đề với vị trí mã, hậu quả và đề xuất sửa.
- [x] Nhiệm vụ 2: tách class theo trách nhiệm và truyền phụ thuộc qua interface, constructor.
- [ ] Nhiệm vụ 3: Factory, Decorator, Strategy, Observer. Các vị trí TODO nằm tại `Program.cs`, `EventService.cs` và `ReportService.cs`.
- [ ] Nhiệm vụ 4: hoàn thiện `Event.Register`, `Event.CheckIn` và các test Domain theo Arrange–Act–Assert.
- [ ] Nhiệm vụ 5: đủ ít nhất 5 commit có ý nghĩa, rebase lên `main`, mở Pull Request và có ít nhất 2 nhận xét tự review.

## Nộp bài

Làm trên nhánh `ch02_0024419387_feature/solid-refactor`, commit theo Conventional Commits, rebase lên `main` rồi mở Pull Request với mô tả What, Why, How to test. Nộp link Pull Request theo hướng dẫn trên LMS.
