using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
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
                // Thay vì next24Hours, lấy toàn bộ lịch của ngày hôm nay
                var criteria = CriteriaOperator.Parse(
            "NotificationSent = ? AND [NgayHoc] = ?",
            false, now.Date);  // Chỉ kiểm tra theo ngày, không quan tâm giờ

                var upcomingClasses = objectSpace.GetObjects<Lichhoc>(criteria);

                foreach (var lichHoc in upcomingClasses)
                {
                    // Thêm log để debug
                    Console.WriteLine($"Found class: {lichHoc.MonhocID?.TenMon} at {lichHoc.NgayHoc:dd/MM/yyyy HH:mm}");
                    Console.WriteLine($"Student: {lichHoc.MaSinhVien?.HoTen}, Email enabled: {lichHoc.MaSinhVien?.EnableEmailNotification}");

                    if (lichHoc.MaSinhVien?.EnableEmailNotification == true)
                    {
                        SendNotification(lichHoc);
                        lichHoc.NotificationSent = true;
                    }
                }

                objectSpace.CommitChanges();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }

        private void SendNotification(Lichhoc lichHoc)
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
                    <p><strong>Thời gian:</strong> {lichHoc.NgayHoc}</p>
                    <p><strong>Thời gian:</strong> {lichHoc.GioHoc}</p>
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