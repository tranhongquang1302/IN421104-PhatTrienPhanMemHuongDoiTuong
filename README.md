# Bài tập Ch05 — Authentication và Security: JWT + QR Check-in (starter)

Đề bài đầy đủ: `../de-bai.html`. Rubric: `../rubric.html`. Đánh giá HĐ1.3 (Bài tập chương), CLO2, CLO4, CLO5, CLO7.

## Chạy thử

```bash
cd 02_Projects/ch05/starter
docker compose up -d postgres            # PostgreSQL 16, DB "eduhub", mật khẩu mặc định "secret"
dotnet build                             # starter build được ngay
dotnet user-secrets set "JwtSettings:Secret" "chuoi-ngau-nhien-it-nhat-32-ky-tu-abcdef" --project EduHub.API
dotnet test                              # 9 test đỏ cho đến khi bạn làm xong các TODO 6.x
dotnet run --project EduHub.API          # mở /swagger
```

Có thể thay user-secrets bằng biến môi trường `JwtSettings__Secret` (xem `.env.example`).
Không commit secret thật.

## Danh sách TODO

Mỗi `// TODO x.y` trong mã nguồn thuộc một nhiệm vụ của đề bài. Tìm nhanh bằng cách tìm `TODO` trong thư mục.

| Nhiệm vụ | TODO | Nằm ở |
|---|---|---|
| 1. Argon2id | 1.1 đến 1.6 | `EduHub.Infrastructure/Auth/Argon2PasswordHasher.cs` |
| | 1.7, 1.8 | `EduHub.API/Program.cs` (đăng ký hasher cho ứng dụng và cho Identity) |
| 2. JWT access + refresh | 2.1 đến 2.3 | `EduHub.Infrastructure/Auth/JwtTokenService.cs` |
| | 2.4 đến 2.6 | `EduHub.Application/Auth/LoginCommand.cs` |
| | 2.7 đến 2.11 | `EduHub.Application/Auth/RefreshCommand.cs` |
| | 2.12, 2.13 | `EduHub.API/Controllers/AuthController.cs` |
| | 2.14 | `EduHub.API/Program.cs` (cấu hình JwtBearer) |
| 3. Identity, role, claim, Policy | 3.1 đến 3.3 | `EduHub.Infrastructure/Persistence/Repositories.cs` |
| | 3.4, 3.5 | `EduHub.API/Security/Policies.cs` |
| | 3.6 đến 3.9 | `ClubsController.cs`, `EventsController.cs` |
| | 3.10, 3.11 | `EduHub.API/Program.cs` (Identity, AddAuthorization) |
| 4. QR check-in | 4.1 đến 4.6 | `EduHub.Infrastructure/Auth/QrTokenService.cs` |
| | 4.7 đến 4.11 | `EduHub.Application/CheckIns/CheckInService.cs` |
| 5. CORS, HTTPS, secret | 5.1 đến 5.3 | `EduHub.API/Program.cs` |
| 6. Test | 6.1 đến 6.9 | `EduHub.Tests/` |

## Đã viết sẵn (không cần sửa)

- Entity `Club`, `Event`, `Registration`, `CheckIn`, `RefreshToken`; `IdentityDbContext`, seed 3 tài khoản mẫu
- `RefreshTokens.Hash` (băm SHA-256 refresh token), `IssueAsync` (phát cặp token), `IdentityArgon2Hasher` (cầu nối Argon2id cho Identity)
- Rate limiting cho đăng nhập và check-in, Swagger có nút Authorize
- Lớp hỗ trợ test: `ManualClock` (đồng hồ giả), `InMemoryRefreshTokenStore`, `TestAccounts`

## Quy ước nộp bài

Nhánh `ch05/<MSSV>`, commit theo Conventional Commits viết bằng tiếng Anh, mở Pull Request về `main` và tag giảng viên.
