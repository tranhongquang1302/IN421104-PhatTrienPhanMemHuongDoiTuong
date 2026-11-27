# EduHub Database · Bài tập Chương 4

IN4211 · Phát triển phần mềm hướng đối tượng · Trường Đại học Đồng Tháp

EduHub cung cấp API quản lý câu lạc bộ, sự kiện và đăng ký tham gia trên .NET 10 và PostgreSQL 16. Domain chứa entity và interface. Application xử lý nghiệp vụ và trả DTO. Infrastructure cài đặt Repository, Unit of Work và truy vấn Dapper. API cấu hình DI, validation, ProblemDetails và Serilog.

## Chạy bằng Docker

Cần Docker Desktop đang chạy. Tại thư mục chứa `EduHub.sln`, mở PowerShell và nhập mật khẩu cho PostgreSQL. Nếu dùng lại volume, nhập mật khẩu đã dùng khi tạo volume.

```powershell
$env:DB_PASSWORD = Read-Host 'Mật khẩu PostgreSQL' -MaskInput
docker compose up -d --build --wait
```

Mở Swagger tại http://localhost:8080/swagger. Đặt `$env:DB_PORT = '5433'` hoặc `$env:API_PORT = '8081'` trước khi chạy nếu cổng mặc định đang được dùng. `docker compose down` dừng dịch vụ và giữ dữ liệu.

API áp migration `Init` và seed khi bật `Database:AutoMigrate`. Compose và cấu hình Development bật tùy chọn này. Lần chạy đầu tạo ba câu lạc bộ, hai sự kiện và ba thành viên. Lần chạy sau bỏ qua seed khi đã có câu lạc bộ.

## Chạy API bằng .NET SDK

```powershell
docker compose up -d --wait postgres
$ch04DbPort = if ($env:DB_PORT) { $env:DB_PORT } else { '5432' }
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Port=$ch04DbPort;Database=eduhub;Username=postgres;Password=`"$env:DB_PASSWORD`""
dotnet run --project EduHub.API --urls http://localhost:5000
```

Mở Swagger tại http://localhost:5000/swagger. Dùng thời gian ISO 8601 có múi giờ khi gửi `startTime`.

## API

| Phương thức | Đường dẫn | Kết quả |
| --- | --- | --- |
| GET, POST | `/api/clubs` | 200 danh sách, 201 tạo |
| GET, PUT, DELETE | `/api/clubs/{id}` | 200 chi tiết hoặc cập nhật, 204 xóa |
| GET, POST | `/api/events` | 200 danh sách, 201 tạo |
| GET, PUT, DELETE | `/api/events/{id}` | 200 chi tiết hoặc cập nhật, 204 xóa |
| GET, POST | `/api/events/{eventId}/registrations` | 200 danh sách kèm thông tin thành viên, 201 đăng ký |
| DELETE | `/api/events/{eventId}/registrations/{registrationId}` | 204 hủy |
| GET | `/api/reports/events?minRegistered=0` | 200 báo cáo số lượt đăng ký còn hiệu lực và tỷ lệ lấp đầy |

Danh sách câu lạc bộ và sự kiện hỗ trợ `search`, `page` và `pageSize`. Danh sách sự kiện có thêm `clubId`. Kết quả gồm `items`, `totalCount`, `page`, `pageSize` và `totalPages`. Repository lọc, sắp xếp và cắt trang trong SQL.

API trả `400` cho dữ liệu sai, sự kiện đã đầy hoặc sức chứa mới thấp hơn số lượt đăng ký còn hiệu lực. API trả `404` khi không tìm thấy tài nguyên và `409` khi thành viên đã có đăng ký cho sự kiện. Hủy chuyển trạng thái sang `Cancelled` và giải phóng chỗ. Bản ghi đã hủy vẫn xuất hiện trong danh sách để tra cứu và vẫn được tính theo ràng buộc mỗi thành viên chỉ đăng ký một lần cho mỗi sự kiện. Báo cáo chỉ đếm lượt còn hiệu lực và giữ cả sự kiện có 0 lượt khi `minRegistered=0`.

Lỗi trả `application/problem+json` kèm `traceId`. Log JSON ghi mã truy vết, mã trạng thái HTTP và các thuộc tính nghiệp vụ như `EventId`, `RegistrationId` khi tương ứng.

## Kiểm thử

```powershell
dotnet build EduHub.sln
dotnet test EduHub.sln
dotnet tool install --global dotnet-ef --version '10.*'
dotnet ef migrations has-pending-model-changes --project EduHub.Infrastructure --startup-project EduHub.API
```

Bốn test Moq kiểm tra đăng ký thành công và ghi đúng một lần, thiếu sự kiện, trùng đăng ký và hết chỗ. Hai test Domain kiểm tra sức chứa. Migration `Init` có sẵn trong mã nguồn. Khi thay đổi mô hình, sinh migration tiếp theo bằng công cụ EF Core. [Tài liệu quản lý migration của EF Core](https://learn.microsoft.com/ef/core/managing-schemas/migrations/managing)

Để thử trên Swagger, lấy `id` của sự kiện **Hackathon nhỏ** qua `GET /api/events` và lấy ID của ba thành viên mẫu.

```powershell
'SELECT "Id", "StudentId" FROM members ORDER BY "StudentId";' |
    docker compose exec -T postgres psql -U postgres -d eduhub
```

Đăng ký lần lượt **SV001** → **SV001 lần nữa** → **SV002** → **SV003** để quan sát `201`, `409`, `201`, `400`. Hủy đăng ký của SV001 bằng ID trả về từ lần đăng ký đầu, sau đó thử SV003 lại. Kiểm tra danh sách đăng ký, báo cáo và `docker compose logs api` để đối chiếu mã truy vết.

## Hai câu hỏi thiết kế

1. `IEventRepository` biểu đạt các truy vấn dành riêng cho Event như lọc theo câu lạc bộ, phân trang và lấy sự kiện để kiểm tra sức chứa trong giao dịch. Với Member, thao tác duy nhất là kiểm tra tồn tại khi đăng ký và `IRegistrationRepository.MemberExistsAsync` đã đáp ứng. Một `IRepository<T>` tổng quát sẽ lặp API của `DbSet<T>` mà vẫn cần thêm các phương thức nghiệp vụ riêng.
2. Giữ `IUnitOfWork` giúp Application kiểm soát thời điểm lưu, gom thay đổi của một use case và kiểm thử bằng Moq mà không cần tham chiếu EF Core. Có thể bỏ lớp bọc này khi chọn dùng trực tiếp EF Core trong application vì `DbContext` đã theo dõi thay đổi và cung cấp `SaveChangesAsync` cùng API giao dịch.

Luồng đăng ký và luồng cập nhật sức chứa khóa cùng dòng Event trong suốt giao dịch. Các yêu cầu cạnh tranh kiểm tra số lượt đăng ký lần lượt, vì thế chỉ một yêu cầu có thể lấy chỗ cuối. [Tài liệu khóa dòng của PostgreSQL](https://www.postgresql.org/docs/16/explicit-locking.html)
