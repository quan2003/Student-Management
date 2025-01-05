using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using StudentManagement.Module.BusinessObjects;
using StudentManagement.Module.BusinessObjects.StudentManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudentManagement.Module.Controllers
{
    public partial class DiemdanhListViewController : ViewController
    {
        private SimpleAction diemDanhNhomAction;
        private SimpleAction showTodayAttendanceAction;
        private SimpleAction sendAbsentNotificationAction;

        public DiemdanhListViewController()
        {
            InitializeComponent();

            TargetViewType = ViewType.ListView;
            TargetObjectType = typeof(Diemdanh);

            diemDanhNhomAction = new SimpleAction(this, "DiemDanhNhomAction", PredefinedCategory.RecordEdit)
            {
                Caption = "Điểm Danh Nhóm",
                ConfirmationMessage = "Xác nhận điểm danh cho nhóm sinh viên đã chọn?",
                ImageName = "Action_MultipleEdit",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ToolTip = "Điểm danh cho nhiều sinh viên"
            };

            showTodayAttendanceAction = new SimpleAction(this, "ShowTodayAttendanceAction", PredefinedCategory.View)
            {
                Caption = "Điểm Danh Hôm Nay",
                ImageName = "Action_Calendar",
                ToolTip = "Hiển thị danh sách điểm danh ngày hôm nay"
            };

            sendAbsentNotificationAction = new SimpleAction(this, "SendAbsentNotification", PredefinedCategory.Tools)
            {
                Caption = "Gửi Thông Báo Vắng Học",
                ConfirmationMessage = "Xác nhận gửi thông báo vắng học cho sinh viên?",
                ImageName = "Action_SendEmail",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ToolTip = "Gửi email thông báo vắng học cho sinh viên"
            };

            diemDanhNhomAction.Execute += DiemDanhNhomAction_Execute;
            showTodayAttendanceAction.Execute += ShowTodayAttendanceAction_Execute;
            sendAbsentNotificationAction.Execute += SendAbsentNotificationAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            // Chỉ thực hiện khi là ListView và không phải lần đầu load
            if (View is ListView && Application.MainWindow != null)
            {
                try
                {
                    // Áp dụng filter cho ngày hôm nay
                    var today = DateTime.Today;
                    var criteria = CriteriaOperator.Parse("[LichhocID.NgayHoc] = ?", today);
                    ((ListView)View).CollectionSource.Criteria["Filter"] = criteria;

                    // Tạo điểm danh cho các lịch học hôm nay nếu chưa có
                    CreateAttendanceForToday();
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"Lỗi khi tải dữ liệu: {ex.Message}");
                }
            }
        }
        private void CreateAttendanceForToday()
        {
            var objectSpace = View.ObjectSpace;
            var today = DateTime.Today;

            // Lấy danh sách lịch học hôm nay mà đã có sinh viên
            var todaySchedules = objectSpace.GetObjects<Lichhoc>(CriteriaOperator.Parse(
                "NgayHoc = ? AND MaSinhVien IS NOT NULL AND MaSinhVien.MaSinhVien IS NOT NULL",
                today));

            bool needsCommit = false;

            foreach (var schedule in todaySchedules)
            {
                // Kiểm tra xem đã có điểm danh cho lịch học này chưa
                var existingAttendance = objectSpace.FindObject<Diemdanh>(CriteriaOperator.Parse(
                    "LichhocID = ? AND MaSinhvien = ?",
                    schedule, schedule.MaSinhVien));

                if (existingAttendance == null)
                {
                    var newAttendance = objectSpace.CreateObject<Diemdanh>();
                    newAttendance.LichhocID = schedule;
                    newAttendance.MaSinhvien = schedule.MaSinhVien;
                    newAttendance.TrangThaiDiemDanh = false;
                    needsCommit = true;
                }
            }

            if (needsCommit)
            {
                try
                {
                    objectSpace.CommitChanges();
                    View.Refresh();
                }
                catch (Exception ex)
                {
                    objectSpace.Rollback();
                    throw new UserFriendlyException($"Lỗi khi tạo điểm danh: {ex.Message}");
                }
            }
        }

        private void ShowTodayAttendance()
        {
            if (!(View is ListView listView)) return;

            var objectSpace = View.ObjectSpace;
            var today = DateTime.Today;

            // Lọc điểm danh theo ngày hôm nay
            var criteria = CriteriaOperator.Parse("[LichhocID.NgayHoc] = ?", today);
            listView.CollectionSource.Criteria["Filter"] = criteria;

            // Tạo điểm danh cho các sinh viên chưa có record
            var todaySchedules = objectSpace.GetObjects<Lichhoc>(CriteriaOperator.Parse("NgayHoc = ?", today));

            foreach (var schedule in todaySchedules)
            {
                var student = schedule.MaSinhVien;
                if (student != null)
                {
                    var existingAttendance = objectSpace.FindObject<Diemdanh>(CriteriaOperator.Parse(
                        "LichhocID = ? AND MaSinhvien = ?",
                        schedule, student));

                    if (existingAttendance == null)
                    {
                        var newAttendance = objectSpace.CreateObject<Diemdanh>();
                        newAttendance.LichhocID = schedule;
                        newAttendance.MaSinhvien = student;
                        newAttendance.TrangThaiDiemDanh = false;
                    }
                }
            }

            objectSpace.CommitChanges();
            View.Refresh();
        }

        private void ShowTodayAttendanceAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                ShowTodayAttendance();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi hiển thị điểm danh: {ex.Message}");
            }
        }

        private void SendAbsentNotificationAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var selectedDiemdanhs = e.SelectedObjects.Cast<Diemdanh>().ToList();
                var results = SendAbsentNotifications(selectedDiemdanhs);
                ShowEmailResults(results);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }

        private (int success, List<string> errors) SendAbsentNotifications(List<Diemdanh> diemdanhs)
        {
            int successCount = 0;
            var errors = new List<string>();

            var absentStudents = diemdanhs
                .Where(d => !d.TrangThaiDiemDanh)
                .GroupBy(d => d.MaSinhvien?.MaSinhVien)
                .Where(g => g.Key != null);

            foreach (var group in absentStudents)
            {
                var sinhVien = group.First().MaSinhvien;
                if (!IsValidStudent(sinhVien))
                {
                    errors.Add($"Sinh viên {sinhVien?.HoTen ?? "Không xác định"}: Thiếu thông tin email");
                    continue;
                }

                try
                {
                    SendAbsentEmail(group.ToList());
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Sinh viên {sinhVien.HoTen}: {ex.Message}");
                }
            }

            return (successCount, errors);
        }

        private bool IsValidStudent(Sinhvien sinhVien)
        {
            return sinhVien != null && !string.IsNullOrWhiteSpace(sinhVien.Email);
        }

        private void SendAbsentEmail(List<Diemdanh> absences)
        {
            if (!absences.Any()) return;

            var sinhVien = absences.First().MaSinhvien;
            var subject = "Thông báo vắng học";

            // Tạo chi tiết các buổi vắng với định dạng table
            var absentDetails = absences.Select(a => $@"<tr>
        <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{a.LichhocID?.MonhocID?.TenMon ?? "Không xác định"}</td>
        <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: center;'>{(a.ThoiGianDiemDanh != DateTime.MinValue ? a.ThoiGianDiemDanh.ToString("dd/MM/yyyy HH:mm") : "Chưa ghi nhận")}</td>
        <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{(string.IsNullOrEmpty(a.LyDoVangMat) ? "Không có lý do" : a.LyDoVangMat)}</td>
    </tr>").ToList();

            var content = $@"
        <div style='margin: 20px; font-family: Arial, sans-serif; line-height: 1.6;'>
            <p>Kính gửi {sinhVien.HoTen},</p>

            <p>Nhà trường thông báo về việc vắng học của bạn:</p>

            <table style='width: 100%; border-collapse: collapse; margin: 20px 0; background-color: #f9f9f9;'>
                <tr style='background-color: #e74c3c; color: white;'>
                    <th style='padding: 10px; text-align: left;'>Môn học</th>
                    <th style='padding: 10px; text-align: center;'>Thời gian</th>
                    <th style='padding: 10px; text-align: left;'>Lý do vắng</th>
                </tr>
                {string.Join("\n", absentDetails)}
            </table>

            <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                <p style='margin: 0; color: #856404;'>
                    <strong>Lưu ý:</strong> Đây là email tự động. 
                    Vui lòng liên hệ với giáo viên bộ môn hoặc cố vấn học tập nếu có bất kỳ thắc mắc nào.
                </p>
            </div>

            <p style='margin-top: 30px;'>
                Trân trọng,<br/>
                <strong>Phòng Đào tạo</strong>
            </p>
        </div>";

            EmailHelper.SendHtmlEmail(sinhVien.Email, subject, content);
        }

        private void ShowEmailResults((int success, List<string> errors) results)
        {
            if (results.success > 0)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Đã gửi thành công thông báo cho {results.success} sinh viên.",
                    InformationType.Success);
            }

            if (results.errors.Any())
            {
                var errorMessage = string.Join("\n", results.errors);
                Application.ShowViewStrategy.ShowMessage(
                    $"Không thể gửi thông báo cho một số sinh viên:\n{errorMessage}",
                    InformationType.Error);
            }
        }

        private void DiemDanhNhomAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var selectedDiemdanhs = e.SelectedObjects.Cast<Diemdanh>();
            try
            {
                foreach (var diemdanh in selectedDiemdanhs)
                {
                    UpdateDiemDanh(diemdanh);
                }
                View.ObjectSpace.CommitChanges();
                View.Refresh();
            }
            catch (Exception ex)
            {
                View.ObjectSpace.Rollback();
                throw new UserFriendlyException($"Lỗi điểm danh nhóm: {ex.Message}");
            }
        }

        private void UpdateDiemDanh(Diemdanh diemdanh)
        {
            diemdanh.TrangThaiDiemDanh = !diemdanh.TrangThaiDiemDanh;
            diemdanh.ThoiGianDiemDanh = DateTime.Now;
            if (diemdanh.TrangThaiDiemDanh)
            {
                diemdanh.LyDoVangMat = null;
            }
        }

        protected override void OnDeactivated()
        {
            diemDanhNhomAction.Execute -= DiemDanhNhomAction_Execute;
            showTodayAttendanceAction.Execute -= ShowTodayAttendanceAction_Execute;
            sendAbsentNotificationAction.Execute -= SendAbsentNotificationAction_Execute;
            base.OnDeactivated();
        }
    }
}