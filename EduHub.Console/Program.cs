using System.Text;
using EduHub.Domain.Entities;
using EduHub.Domain.Enums;

// Nhiem vu 4 - Console Demo: 7 buoc nghiep vu + buoc 8 kiem tra luat.
// Program.cs chi dieu phoi; cac quy tac nghiep vu nam trong Domain entities.
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("EDUHUB - CONSOLE DEMO (IN4211 / CHUONG 1)");

// Buoc 1: Tao cau lac bo.
PrintStep(1, "Tạo câu lạc bộ");
Club club = Club.Create("CLB Trí tuệ nhân tạo", "Học tập và nghiên cứu về AI");
Console.WriteLine($"Đã tạo: {club.Name}");

// Buoc 2: Tao ba sinh vien va them vao cau lac bo.
PrintStep(2, "Thêm 3 thành viên");
Member[] students =
{
    Member.Create("Trần Hồng Quang", "quang@example.com", "0024419387"),
    Member.Create("Nguyễn Minh Anh", "minhanh@example.com", "SV00002"),
    Member.Create("Lê Hoàng Nam", "hoangnam@example.com", "SV00003")
};

foreach (Member student in students)
{
    club.AddMember(student);
    Console.WriteLine($"Đã thêm: {student.Name} ({student.StudentCode})");
}
Console.WriteLine($"Tổng thành viên CLB: {club.MemberCount()}");

// Buoc 3: Tao su kien co suc chua 3 nguoi.
PrintStep(3, "Tạo sự kiện");
EduHub.Domain.Entities.Event workshop = EduHub.Domain.Entities.Event.Create(
    club.Id,
    "Hội thảo AI",
    DateTime.UtcNow.AddDays(7),
    capacity: 3,
    isOnline: false
);
Console.WriteLine($"Sự kiện: {workshop.Title} | Sức chứa: {workshop.Capacity}");

// Buoc 4: Moi sinh vien dang ky -> tao Registration (Pending).
PrintStep(4, "Đăng ký sự kiện (Pending)");
var registrations = new List<Registration>();
foreach (Member student in students)
{
    // Chi dem cac phieu dang ky chua bi huy CUA DUNG SU KIEN nay.
    int activeCount = registrations.Count(r =>
        r.EventId == workshop.Id && r.Status != RegistrationStatus.Cancelled);

    Registration registration = Registration.Create(student, workshop, activeCount);
    registrations.Add(registration);
    Console.WriteLine($"{student.Name}: {ToVietnamese(registration.Status)}");
}

// Buoc 5: Xac nhan hai phieu, phieu thu ba van Pending.
PrintStep(5, "Xác nhận đăng ký (Confirmed)");
registrations[0].Confirm();
registrations[1].Confirm();
Console.WriteLine($"{students[0].Name}: {ToVietnamese(registrations[0].Status)}");
Console.WriteLine($"{students[1].Name}: {ToVietnamese(registrations[1].Status)}");
Console.WriteLine($"{students[2].Name}: {ToVietnamese(registrations[2].Status)}");

// Buoc 6: Nguoi thu nhat check-in qua QR.
PrintStep(6, "Check-in qua QR");
var checkIns = new List<CheckIn>();
CheckIn checkIn = CheckIn.Create(registrations[0], method: "QR");
checkIns.Add(checkIn);
Console.WriteLine($"Đã check-in: {students[0].Name} | Phương thức: {checkIn.Method}");

// Buoc 7: In bao cao tham du, phan biet da va chua check-in.
PrintStep(7, "Báo cáo tham dự");
PrintAttendanceReport(workshop, students, registrations, checkIns);

// Buoc 8 (them theo slide trang 65-66): thu vi pham luat nghiep vu.
PrintStep(8, "Kiểm tra các quy tắc nghiệp vụ");
ExpectBusinessError("Không cho thêm cùng một thành viên hai lần", () =>
    club.AddMember(students[0]));
ExpectBusinessError("Không check-in phiếu Pending", () =>
    CheckIn.Create(registrations[2], "QR"));

registrations[2].Cancel();
Console.WriteLine($"Đã hủy phiếu thứ ba: {ToVietnamese(registrations[2].Status)}");
ExpectBusinessError("Không xác nhận lại phiếu Cancelled", () =>
    registrations[2].Confirm());

Console.WriteLine("\nKẾT THÚC DEMO - TẤT CẢ KIỂM TRA VI PHẠM ĐỀU BỊ TỪ CHỐI ĐÚNG.");

static void PrintStep(int number, string title)
{
    Console.WriteLine($"\n=== BƯỚC {number}: {title} ===");
}

static string ToVietnamese(RegistrationStatus status) => status switch
{
    RegistrationStatus.Pending => "Pending (chờ xác nhận)",
    RegistrationStatus.Confirmed => "Confirmed (đã xác nhận)",
    RegistrationStatus.Cancelled => "Cancelled (đã hủy)",
    _ => status.ToString()
};

static void PrintAttendanceReport(
    EduHub.Domain.Entities.Event workshop,
    IReadOnlyList<Member> students,
    IReadOnlyList<Registration> registrations,
    IReadOnlyList<CheckIn> checkIns)
{
    var activeRegistrations = registrations
        .Where(r => r.EventId == workshop.Id && r.Status != RegistrationStatus.Cancelled)
        .ToList();

    int checkedInCount = activeRegistrations.Count(r =>
        checkIns.Any(c => c.RegistrationId == r.Id));

    Console.WriteLine($"Tên sự kiện: {workshop.Title}");
    Console.WriteLine($"Số người đăng ký / sức chứa: {activeRegistrations.Count}/{workshop.Capacity}");
    Console.WriteLine($"Đã check-in: {checkedInCount}");
    Console.WriteLine($"Chưa check-in: {activeRegistrations.Count - checkedInCount}");
    Console.WriteLine("Danh sách người đăng ký:");

    foreach (Registration registration in activeRegistrations)
    {
        // Tra theo khoa ngoai MemberId (chua co database trong Chuong 1).
        Member student = students.Single(m => m.Id == registration.MemberId);
        bool checkedIn = checkIns.Any(c => c.RegistrationId == registration.Id);
        Console.WriteLine($"- {student.Name} | {ToVietnamese(registration.Status)} | " +
                          (checkedIn ? "Đã check-in" : "Chưa check-in"));
    }
}

static void ExpectBusinessError(string scenario, Action action)
{
    try
    {
        action();
        throw new Exception($"KIỂM TRA THẤT BẠI: hệ thống đã chấp nhận thao tác sai ({scenario}).");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"[ĐÚNG] {scenario} -> Bị từ chối: {ex.Message}");
    }
}
