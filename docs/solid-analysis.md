# Phân tích vi phạm SOLID trong ClubManager

**MSSV:** 0024419387 · **GitHub:** tranhongquang1302

**Bài tập:** Chương 2, nhiệm vụ 1, Refactoring EduHub

`ClubManager` đang gộp quản lý CLB, thành viên, sự kiện, lưu dữ liệu, ghi log, gửi thông báo và xuất báo cáo. Sáu vấn đề dưới đây chỉ ra vị trí mã, nguyên lý bị vi phạm, hậu quả và hướng refactor để giữ các chức năng nghiệp vụ nhưng giảm phụ thuộc giữa chúng.

Số dòng được đối chiếu với bản mã gốc trong commit starter, qua [ClubManager.cs của starter](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs), trước khi thực hiện nhiệm vụ refactor.

## 1. CreateClub có nhiều lý do để thay đổi

**Vị trí:** [dòng 35–64](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L35-L64), phương thức `CreateClub`.

**Nguyên lý:** SRP, Single Responsibility Principle. Một class nên có một lý do để thay đổi.

Phương thức vừa kiểm tra tên ở dòng 38–41, tạo và lưu CLB ở dòng 44–45, ghi file ở dòng 48, vừa chọn cách gửi thông báo ở dòng 52–63. Đây là các trách nhiệm có thể thay đổi độc lập: quy tắc đặt tên, cách lưu dữ liệu, cách ghi log và kênh thông báo.

**Hậu quả:** Một thay đổi về gửi tin hoặc lưu trữ cũng buộc sửa class xử lý nghiệp vụ CLB. Kiểm thử việc tạo CLB đồng thời phát sinh ghi file và xuất Console, khiến việc kiểm tra từng hành vi khó tách biệt.

**Đề xuất sửa:** Chuyển quy tắc tên về `Club.Create`; giao việc lưu cho `IClubRepository`, ghi log cho `IAppLogger`, gửi tin cho `INotificationService`. `ClubService` nhận các interface qua constructor và điều phối use case tạo CLB.

## 2. CreateClub lựa chọn thông báo bằng chuỗi if else

**Vị trí:** [dòng 52–63](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L52-L63), các nhánh kiểm tra Gmail, Yahoo và email khác.

**Nguyên lý:** OCP, Open/Closed Principle. Thiết kế cần cho phép mở rộng hành vi mà hạn chế sửa logic đã ổn định.

`CreateClub` biết trực tiếp từng trường hợp thông báo thông qua `presidentEmail.Contains(...)` và gọi `Console.WriteLine` tương ứng.

**Hậu quả:** Khi bổ sung kênh SMS hoặc Slack, phải sửa ngay phương thức tạo CLB. Nghiệp vụ tạo CLB bị gắn với danh sách các cách gửi tin, làm tăng phạm vi cần kiểm tra mỗi lần thêm kênh.

**Đề xuất sửa:** Tạo `INotificationService` với các bản cài đặt Email, SMS và Slack; dùng `INotificationServiceFactory.Create(NotificationChannel)` để tạo đúng dịch vụ. Việc chọn và ráp kênh thuộc composition root hoặc phần cấu hình. `ClubService` chỉ gọi dịch vụ qua interface. Với Simple Factory dạng `switch`, thêm kênh vẫn cần bổ sung nhánh trong factory, nhưng logic tạo CLB được giữ nguyên.

## 3. Logic nghiệp vụ phụ thuộc trực tiếp vào ghi file

**Vị trí:** [dòng 32](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L32), [48](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L48), [79](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L79), [94](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L94) và [111](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L111).

**Nguyên lý:** DIP, Dependency Inversion Principle. Logic cấp cao cần phụ thuộc vào abstraction thay vì chi tiết hạ tầng cụ thể.

`ClubManager` giữ đường dẫn `club-log.txt` và trực tiếp gọi `File.AppendAllText` trong các thao tác tạo CLB, thêm thành viên, tạo sự kiện và in báo cáo.

**Hậu quả:** Đổi từ file sang Console hoặc một hệ thống log khác phải sửa nhiều phương thức nghiệp vụ. Test cần quyền ghi file và có thể để lại dữ liệu dùng chung. Nếu ghi log thất bại sau `_clubs.Add` hoặc `_members.Add`, phương thức ném lỗi dù dữ liệu trong bộ nhớ đã thay đổi.

**Đề xuất sửa:** Định nghĩa `IAppLogger` và truyền qua constructor; triển khai `FileLogger`, `ConsoleLogger` bên ngoài service. Test có thể truyền logger giả. Với thao tác repository, dùng `LoggingClubRepository` theo Decorator để thêm ghi log qua `IAppLogger`; quy định rõ cách xử lý lỗi log theo yêu cầu nghiệp vụ.

## 4. AddMemberToClub cố định hai kênh thông báo

**Vị trí:** [dòng 67–84](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L67-L84), đặc biệt hai lệnh gửi thông báo ở dòng 82–83.

**Nguyên lý:** OCP; đồng thời việc tự gửi tin làm tăng trách nhiệm của class theo SRP.

Sau khi thêm thành viên, phương thức luôn thực hiện thông báo Email và Slack. Danh sách kênh nhận tin được viết cố định ngay trong nghiệp vụ thêm thành viên.

**Hậu quả:** Thêm SMS, bỏ Slack hoặc thay đổi tổ hợp kênh đều cần sửa `AddMemberToClub`, dù quy tắc thêm thành viên không thay đổi. Khó kiểm thử riêng việc giới hạn số thành viên với việc gửi tin.

**Đề xuất sửa:** Cho `ClubService` nhận các dịch vụ thông báo qua interface và cấu hình các kênh tại `Program.cs`. Có thể nhận một tập `INotificationService` để gửi qua các kênh được cấu hình; Factory đảm nhiệm việc tạo từng dịch vụ. Quy tắc thêm thành viên chỉ phụ thuộc vào hợp đồng thông báo.

## 5. CreateEvent tự xử lý từng bên nhận thông báo

**Vị trí:** [dòng 88–98](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L88-L98), đặc biệt vòng lặp gửi email ở dòng 96–97.

**Nguyên lý:** SRP và OCP.

Phương thức vừa tạo sự kiện, ghi log, vừa tìm thành viên của CLB và tự gửi thông báo cho từng người. Nó biết cụ thể hành vi cần chạy sau khi sự kiện được tạo.

**Hậu quả:** Bổ sung bên nhận tin hoặc một hành vi sau tạo sự kiện, như gửi Slack hay cập nhật lịch, sẽ buộc sửa `CreateEvent`. Logic tạo sự kiện và danh sách phản ứng sau đó phát triển cùng trong một phương thức.

**Đề xuất sửa:** Tách `EventService`; áp dụng Observer qua `IEventObserver.OnEventCreated`. Service tạo và lưu sự kiện, sau đó báo cho các observer đã đăng ký. `EmailNotifier` đảm nhiệm tìm người nhận và gửi tin, `LogNotifier` đảm nhiệm log. Thêm observer tại composition root mà không sửa logic tạo sự kiện.

## 6. PrintClubReport gộp tính toán và định dạng đầu ra

**Vị trí:** [dòng 103–113](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L103-L113), phương thức `PrintClubReport`.

**Nguyên lý:** SRP và OCP. Phụ thuộc ghi file ở dòng 111 cũng thuộc vấn đề DIP đã phân tích tại mục 3.

Phương thức vừa duyệt dữ liệu, đếm thành viên ở dòng 105–107, định dạng và xuất báo cáo ra Console ở dòng 108, vừa ghi log ở dòng 111.

**Hậu quả:** Muốn xuất CSV phải sửa hoặc thêm nhánh trong phương thức đang tính thống kê. Thay đổi cách trình bày có thể ảnh hưởng code lấy dữ liệu; khó kiểm thử kết quả thống kê độc lập với đầu ra Console.

**Đề xuất sửa:** Tách `ReportService` lấy dữ liệu qua repository interface và xây dựng kết quả thống kê. Dùng Strategy với `IReportStrategy`, `ConsoleReportStrategy` và `CsvReportStrategy` để định dạng cùng dữ liệu. Giao việc ghi log cho `IAppLogger`. Khi thêm định dạng, bổ sung strategy và chọn tại nơi sử dụng.

## Đánh giá LSP và ISP

Các vi phạm trực tiếp ở trên tập trung vào SRP, OCP và DIP. `ClubManager` hiện không kế thừa class hoặc triển khai interface, nên chưa có trường hợp thay thế kiểu cha để kết luận vi phạm LSP, cũng chưa có interface buộc một client cài đặt các phương thức không dùng để kết luận vi phạm ISP.

Khi refactor, cần thiết kế các interface nhỏ theo nhu cầu như `IClubRepository`, `IMemberRepository`, `IEventRepository`, `IAppLogger` và `INotificationService` để đáp ứng ISP. Các bản cài đặt phải giữ đúng hợp đồng để đáp ứng LSP; ví dụ repository chỉ đọc nên triển khai interface đọc riêng, thay vì hứa hỗ trợ `Add` rồi ném `NotSupportedException`.

## Vấn đề bổ sung về đóng gói

[Dòng 116](https://github.com/tranhongquang1302/IN421104-PhatTrienPhanMemHuongDoiTuong/blob/ffb68b78e97db72a0ed8db149b849f0810337895/Ch02.Refactor/ClubManager.cs#L116) trả thẳng `_clubs` qua `GetAllClubs()`. Code gọi có thể `Clear`, `Add` hoặc `Remove` trực tiếp và bỏ qua quy trình nghiệp vụ. Nên trả snapshot hoặc collection chỉ đọc qua hợp đồng truy vấn repository; cần lưu ý collection chỉ đọc vẫn có thể chứa entity có thuộc tính thay đổi được. Đây là vấn đề đóng gói cần xử lý khi refactor.

## Trách nhiệm sau refactor

| Thành phần | Trách nhiệm |
|---|---|
| `Club` trong Domain | Giữ quy tắc tên và giới hạn thành viên. |
| `ClubService` | Điều phối nghiệp vụ CLB và thành viên. |
| `EventService` | Điều phối sự kiện và phát thông báo cho observer. |
| `ReportService` và `IReportStrategy` | Tách việc tạo dữ liệu thống kê khỏi định dạng báo cáo. |
| Các repository interface | Cung cấp hợp đồng lưu và truy vấn từng loại dữ liệu. |
| `IAppLogger` và `INotificationService` | Tách ghi log, gửi tin khỏi nghiệp vụ. |
| `Program.cs` | Tạo các bản cài đặt cụ thể và truyền phụ thuộc vào service. |

Khi triển khai các nhiệm vụ tiếp theo, cần giữ các hành vi hiện có: tên CLB bắt buộc và tối đa 100 ký tự, mỗi CLB tối đa 50 thành viên, thao tác với CLB không tồn tại phải báo lỗi. Các giới hạn này nên tập trung ở Domain và được kiểm thử cả trường hợp hợp lệ, lỗi và giá trị biên.
