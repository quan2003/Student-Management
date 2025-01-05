using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using StudentManagement.Module.BusinessObjects.StudentManagement;
using StudentManagement.Module.BusinessObjects;

namespace StudentManagement.Module.Services
{
    public class NotificationService
    {
        private readonly IObjectSpace objectSpace;

        public NotificationService(IObjectSpace objectSpace)
        {
            this.objectSpace = objectSpace;
        }

        public void SendUpcomingClassNotifications()
        {
            try
            {
                var now = DateTime.Now;
                // Lấy lịch học trong 24h tới
                var next24Hours = now.AddHours(24);

                Console.WriteLine($"Checking for classes from {now} to {next24Hours}");

                // Sửa lại criteria chỉ check thời gian, không check NotificationSent
                var criteria = CriteriaOperator.Parse(
                    "[NgayHoc] >= ? AND [NgayHoc] <= ?",
                    now, next24Hours);

                var upcomingClasses = objectSpace.GetObjects<Lichhoc>(criteria);
                Console.WriteLine($"Found {upcomingClasses.Count} upcoming classes");

                foreach (var lichHoc in upcomingClasses)
                {
                    Console.WriteLine($"Processing class: {lichHoc.MonhocID?.TenMon} at {lichHoc.NgayHoc}");

                    if (lichHoc.MaSinhVien?.EnableEmailNotification == true &&
                        !string.IsNullOrWhiteSpace(lichHoc.MaSinhVien.Email))
                    {
                        try
                        {
                            SendNotification(lichHoc);
                            lichHoc.NotificationSent = true;
                            Console.WriteLine($"Sent notification for class {lichHoc.MonhocID?.TenMon}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending notification: {ex.Message}");
                        }
                    }
                }

                objectSpace.CommitChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendUpcomingClassNotifications: {ex.Message}");
                throw;
            }
        }

        public void SendNotification(Lichhoc lichHoc) // Thêm public để có thể gọi từ controller
        {
            var sinhVien = lichHoc.MaSinhVien;
            if (sinhVien == null || string.IsNullOrWhiteSpace(sinhVien.Email))
                return;

            var subject = "Thông báo lịch học sắp tới";
            var content = $@"
            <div style='margin: 20px; font-family: Arial, sans-serif;'>
                <p>Kính gửi {sinhVien.HoTen},</p>
                <p>Nhà trường xin thông báo về buổi học sắp diễn ra:</p>
                <div style='margin: 20px 0; padding: 15px; background-color: #f5f5f5; border-radius: 5px;'>
                    <p><strong>Môn học:</strong> {lichHoc.MonhocID?.TenMon}</p>
                    <p><strong>Ngày:</strong> {lichHoc.NgayHoc}</p>
                    <p><strong>Giờ:</strong> {lichHoc.GioHoc}</p>
                    <p><strong>Phòng học:</strong> {lichHoc.PhongHoc}</p>
                </div>
                <p>Vui lòng đến lớp đúng giờ.</p>
                <p style='margin-top: 30px;'>
                    Trân trọng,<br/>
                    <em>Phòng Đào tạo</em>
                </p>
            </div>";

            EmailHelper.SendHtmlEmail(sinhVien.Email, subject, content);
        }
    }
}