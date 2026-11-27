# Ch06 — Testing, Docker và CI/CD (starter)

Starter của Bài tập Ch06 — Testing, Docker và CI/CD: EduHub Production-Ready (HĐ1.3). Mã nguồn EduHub (kế thừa Ch05: JWT, Argon2id, Identity, QR check-in, cùng handler Club, Event, Registration) đã chạy được; bạn hoàn thiện các đoạn `TODO` bên dưới. Xem đề bài và rubric trong `de-bai.html`, `rubric.html`.

## Danh sách TODO

### Nhiệm vụ 1 — Unit test (`EduHub.Tests`, coverage từ 50%)

- [ ] `Clubs/CreateClubCommandTests.cs`
- [ ] `Events/CreateEventCommandTests.cs`
- [ ] `Events/CheckInTests.cs`
- [ ] `Registrations/RegisterForEventTests.cs`
- Test mẫu: `Clubs/GetClubsQueryTests.cs`. Xóa `Skip = ...` khi đã viết xong từng test.
- Các test của Ch05 (`Auth/`, `CheckIns/`) đã xanh sẵn, không cần sửa.

### Nhiệm vụ 2 — Integration test

- [ ] `Integration/ClubsApiTests.cs`: GET /api/clubs (200), POST /api/clubs (201), GET /api/clubs/{id} không tồn tại (404). API yêu cầu đăng nhập: dùng `factory.ClientWith(factory.AdminToken)` (đã có sẵn trong `ApiFactory`)

### Nhiệm vụ 3 — Dockerfile

- [ ] `EduHub.API/Dockerfile`: chuyển thành multi-stage (sdk Alpine rồi aspnet Alpine)

### Nhiệm vụ 4 — Docker Compose

- [ ] `docker-compose.yml`: đọc biến từ `.env`, thêm healthcheck cho `api` và `postgres`

### Nhiệm vụ 5 — GitHub Actions

- [ ] `.github/workflows/ci.yml`: thêm job `push-image` (build → test → push image)
- [ ] Thêm huy hiệu CI vào README của repo bạn

## Lệnh thường dùng

```bash
# Test + coverage
dotnet test EduHub.sln --collect:"XPlat Code Coverage" --results-directory ./coverage

# Báo cáo HTML (cần cài reportgenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"./coverage/**/coverage.cobertura.xml" -targetdir:"./coverage/html" -reporttypes:Html

# Docker
docker build -f EduHub.API/Dockerfile -t eduhub-api .
# .env.example có DB_PASSWORD và JwtSettings__Secret (khoá JWT >= 32 ký tự)
cp .env.example .env
docker compose up -d --build
curl http://localhost:8080/healthz
```

## Nộp bài

Làm trên nhánh riêng, commit theo Conventional Commits, mở Pull Request về `main` với mô tả what, why, how to test.
