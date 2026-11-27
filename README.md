# EduHub Ch05 — JWT và QR check-in

Bài tập HĐ1.3 của Trần Hồng Quang (MSSV 0024419387). Dự án dùng .NET 10, ASP.NET Core Identity và PostgreSQL.

## Chạy trên máy cục bộ

1. Sao chép `.env.example` thành `.env`, đặt `DB_PASSWORD` cho PostgreSQL và chạy `docker compose up -d postgres`. Nếu cổng 5432 đã được dùng, đặt thêm `DB_PORT` trong `.env` và dùng cùng cổng trong chuỗi kết nối.
2. Từ thư mục chứa `EduHub.sln`, chạy các lệnh dưới đây. Thay mật khẩu bằng giá trị trong `.env` và dùng hai khóa ngẫu nhiên riêng biệt, mỗi khóa tối thiểu 32 byte.

   ```sh
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5432;Database=eduhub;Username=postgres;Password=<DB_PASSWORD>" --project EduHub.API
   dotnet user-secrets set "JwtSettings:Secret" "<khóa JWT ngẫu nhiên>" --project EduHub.API
   dotnet user-secrets set "QrSettings:Secret" "<khóa QR ngẫu nhiên>" --project EduHub.API
   dotnet build EduHub.sln
   dotnet test EduHub.sln --no-build
   dotnet run --project EduHub.API
   ```

3. Mở [Swagger cục bộ](http://localhost:5050/swagger). Khi chạy Development với cơ sở dữ liệu mới, API tạo các tài khoản mẫu sau.

| Email | Mật khẩu mẫu | Quyền |
|---|---|---|
| `admin@eduhub.local` | `Admin@123` | Admin |
| `member@eduhub.local` | `Member@123` | Member |
| `organizer@eduhub.local` | `Organizer@123` | Member có claim `permission=qr.issue` |

Có thể tạo một khóa ngẫu nhiên bằng `openssl rand -base64 32`. Trên PowerShell 7 dùng `[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))`.

Ngoài Development, cung cấp cấu hình qua `ConnectionStrings__DefaultConnection`, `JwtSettings__Secret`, `QrSettings__Secret` và `Cors__AllowedOrigins__0` (thêm chỉ số cho các origin tiếp theo). API chuyển hướng HTTP sang HTTPS và gửi HSTS. Nếu HTTPS dùng cổng khác 443, đặt `ASPNETCORE_HTTPS_PORT` theo cổng đã cấu hình cho máy chủ.

## Endpoint và cách kiểm tra

| Phương thức | Đường dẫn | Kết quả hoặc điều kiện |
|---|---|---|
| POST | `/api/auth/register` | `201` cho tài khoản Member. Mật khẩu yếu hoặc email trùng trả `400`. |
| POST | `/api/auth/login` | `200` với cặp token khi đúng thông tin. Email hoặc mật khẩu sai đều trả `401` cùng thông điệp. |
| POST | `/api/auth/refresh` | Gửi `{"refreshToken":"..."}` để đổi cặp token. Token hết hạn, đã thu hồi hoặc không hợp lệ trả `401`. |
| POST | `/api/auth/logout` | Gửi `{"refreshToken":"..."}` để thu hồi token. Trả `204` cả khi gọi lại. |
| GET | `/api/auth/me`, `/api/clubs`, `/api/events` | Gửi `Authorization: Bearer <accessToken>`. Thiếu token hoặc token sai trả `401`. |
| POST, DELETE | `/api/clubs`, `/api/clubs/{id}` | `AdminOnly`. Member nhận `403`. |
| DELETE | `/api/events/{id}` | `AdminOnly`. |
| POST | `/api/events/{id}/register` | Đăng ký sự kiện cho tài khoản đang đăng nhập. |
| POST | `/api/events/{id}/generate-qr` | `CanIssueQr`. Admin hoặc người có claim `permission=qr.issue` nhận `qrPayload` và `expiresAt`. |
| POST | `/api/events/{id}/checkin` | Gửi `{"qrPayload":"..."}`. Trả `200` khi thành công, `400` khi QR sai hoặc hết hạn, `403` khi chưa đăng ký và `409` khi đã điểm danh. |

Để thử luồng QR trên Swagger, đăng nhập bằng Organizer, dùng access token qua **Authorize** rồi lấy ID sự kiện và tạo QR. Đổi sang access token của Member, đăng ký cùng sự kiện và gửi payload QR hai lần. Lần đầu trả `200`, lần sau trả `409`.

Để thử luồng refresh, đăng nhập và dùng refresh token A đổi lấy B. Gửi lại A để kiểm tra `401`, sau đó gửi B để kiểm tra các refresh token đang hoạt động của tài khoản đã bị thu hồi.

## Cách triển khai

- Identity dùng Argon2id cho mật khẩu mới với salt ngẫu nhiên 16 byte, 64 MiB bộ nhớ, 4 vòng lặp và 2 luồng. Kiểm tra mật khẩu bằng phép so sánh thời gian cố định.
- JWT ký bằng HS256, kiểm tra chữ ký, issuer, audience và thời hạn. Access token mặc định có hạn 60 phút. Refresh token sống 7 ngày, được lưu dưới dạng bản băm SHA-256. Xoay vòng và thu hồi dùng giao dịch PostgreSQL theo tài khoản.
- QR ký bằng HMAC-SHA256 và hết hạn sau 15 phút. Một QR áp dụng cho một sự kiện và mỗi thành viên đã đăng ký được điểm danh một lần. Ràng buộc duy nhất trong PostgreSQL bảo vệ cả khi hai yêu cầu đến đồng thời.
- CORS chỉ cấp quyền cho các origin đã cấu hình, với `GET`, `POST`, `DELETE` và hai header `Authorization`, `Content-Type`. Rate limiter giới hạn 10 yêu cầu/phút/IP cho đăng ký, đăng nhập và refresh, cùng 20 yêu cầu/phút/tài khoản cho check-in. API gửi và nhận token qua JSON hoặc header Bearer. Nếu chuyển refresh token sang cookie, cần bảo vệ các endpoint dùng cookie trước CSRF.
