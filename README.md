# EduHub Ch06 — Production-Ready

[![CI](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/actions/workflows/ci.yml/badge.svg)](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/actions/workflows/ci.yml)

Bài tập HĐ1.3 của Trần Hồng Quang, MSSV 0024419387. Cần .NET 10 SDK để chạy test và Docker với Compose để chạy API cùng PostgreSQL.

## Build và kiểm thử

```sh
dotnet build EduHub.sln -c Release
dotnet test EduHub.sln -c Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage
```

Kết quả kiểm tra gồm 38 ca unit test, 5 ca integration test và 77,93% line coverage (431/553 dòng). Các unit test dùng Moq để kiểm tra tương tác với repository khi tạo câu lạc bộ, tạo sự kiện và đăng ký. Các ca điểm danh kiểm tra thời điểm xác nhận và hành vi khi xác nhận lại.

Integration test chạy ứng dụng thật qua `WebApplicationFactory`, dùng EF Core InMemory và JWT lấy từ endpoint đăng nhập.

| Yêu cầu | Kỳ vọng |
| --- | --- |
| Member gọi `GET /api/clubs` | `200 OK`, có câu lạc bộ trong dữ liệu mẫu |
| Admin gọi `POST /api/clubs` | `201 Created`, nội dung và URL `Location` trỏ đến câu lạc bộ đã lưu |
| Member gọi `GET /api/clubs/{id}` với ID không tồn tại | `404 Not Found` |
| Gọi danh sách câu lạc bộ khi chưa đăng nhập | `401 Unauthorized` |
| Member gửi yêu cầu tạo câu lạc bộ | `403 Forbidden` |

Báo cáo Cobertura nằm trong `coverage/<id>/coverage.cobertura.xml`.

## Chạy API và PostgreSQL bằng Compose

Sao chép `.env.example` thành `.env` (`cp` trên Bash hoặc `Copy-Item` trên PowerShell). Đặt `DB_PASSWORD` và `JwtSettings__Secret` bằng hai chuỗi hex ngẫu nhiên riêng, với khóa JWT ít nhất 32 byte. Có thể đổi `API_PORT` nếu cổng 8080 đang được sử dụng. Git bỏ qua `.env`.

```sh
docker compose up --build --wait
docker compose ps
curl -f http://127.0.0.1:8080/healthz
docker image inspect eduhub-api:local --format '{{.Size}}'
```

Trên PowerShell, dùng `curl.exe` hoặc `Invoke-RestMethod`. Hai dịch vụ phải hiện `healthy`. `/healthz` trả `{"status":"Healthy"}` và kích thước image in theo byte phải dưới 200.000.000. API dùng image ASP.NET Alpine và chạy bằng người dùng `app`.

Swagger tại `http://127.0.0.1:8080/swagger`. Tài khoản dùng thử cục bộ là `admin@eduhub.local` với mật khẩu `Admin@123` và `member@eduhub.local` với mật khẩu `Member@123`. Có thể dùng Bash để chạy `./demo.sh http://127.0.0.1:8080`. Dùng `docker compose down` để dừng hai dịch vụ.

## GitHub Actions

Pull Request về `main` và các lần push vào `main` đều chạy build, test và kiểm tra mức coverage tối thiểu 50%. Workflow tải lên artifact `coverage-report`, khởi động Compose, truy vấn bảng `clubs` trong PostgreSQL và kiểm tra image dưới 200 MB. Khi có push vào `main` và các bước trước đều đạt, job `push-image` đưa image lên `ghcr.io/tranhongquang1302/in421104-phattrienphanmemhuongdoituong` với tag `latest` và `sha-<commit>`.
