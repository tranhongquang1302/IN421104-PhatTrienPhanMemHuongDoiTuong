# Ch04 — EF Core, PostgreSQL và Full API: EduHub Database (starter)

Bài tập Ch04, đánh giá theo HĐ1.3 (Bài tập chương). Đọc đề bài đầy đủ ở `../de-bai.html` và rubric ở `../rubric.html`.
CLO liên quan: CLO1, CLO2, CLO4 (và CLO5, CLO7 qua quy trình Pull Request và tự nghiên cứu).

Starter đã build được. Mọi chỗ cần bạn viết đều có `// TODO [Nhiệm vụ n]` khớp với đề bài.

## Chạy

```bash
docker compose up -d                       # PostgreSQL 16 (DB_PORT=5433 nếu cổng 5432 đã bị chiếm)
dotnet build
dotnet test                                # test mẫu của Domain chạy ngay; các test Skip là việc của bạn
dotnet ef migrations add Init --project EduHub.Infrastructure --startup-project EduHub.API
dotnet run --project EduHub.API --urls http://localhost:5000     # Swagger: http://localhost:5000/swagger
```

Cài công cụ nếu chưa có: `dotnet tool install --global dotnet-ef`.

## Danh sách TODO theo nhiệm vụ

| Nhiệm vụ | Việc cần làm | Nơi làm |
|---|---|---|
| 1. EF Core, PostgreSQL, migration, seeding (20đ) | `OnModelCreating`, `SeedAsync`, `AddDbContext`, tự động migrate khi khởi động, migration `Init` | `EduHubDbContext.cs`, `Program.cs` |
| 2. Repository và Unit of Work (15đ) | `EfUnitOfWork`, các hàm truy vấn của repository, DI | `Repositories/*.cs` |
| 3. CRUD, DTO, validation, phân trang (25đ) | `SearchAsync` (filter, order, skip/take), `EventService`, `ClubService`, `GetEventsQueryHandler`, validation trong `Requests.cs` | `EduHub.Application`, `Requests.cs` |
| 4. Registration API (15đ) | đăng ký, hủy, danh sách đăng ký, các test Moq | `RegisterForEventCommand.cs`, `RegistrationQueries.cs`, `EfRegistrationRepository.cs`, `EduHub.Tests` |
| 5. Dapper cho báo cáo (10đ) | câu SQL và `QueryAsync` | `DapperEventReportQuery.cs` |
| 6. ProblemDetails và Serilog (15đ) | `GlobalExceptionHandler`, `AddProblemDetails`, cấu hình Serilog, log có cấu trúc | `GlobalExceptionHandler.cs`, `Program.cs`, `appsettings.json` |

## Ghi chú thiết kế cần bạn trả lời trong README của repo nộp bài

1. Vì sao dự án này có `IEventRepository` nhưng không tạo `IMemberRepository` hay generic `IRepository<T>`?
2. `IUnitOfWork` có cần thiết không khi `DbContext` đã là Unit of Work? Nêu một lý do giữ và một lý do bỏ.

## Quy trình nộp bài

Làm trên nhánh `feature/ch04-<mssv>`, mỗi tính năng một commit theo Conventional Commits (ví dụ `feat(db): add EF Core mappings and Init migration`), rồi mở Pull Request về `main`, mô tả đủ what, why, how to test.
