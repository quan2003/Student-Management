using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using StudentManagement.Module.BusinessObjects;
using StudentManagement.Module.BusinessObjects.StudentManagement;
using StudentManagement.Module.Services;
using System;
using System.Linq;

namespace StudentManagement.Module.Controllers
{
    public partial class LichhocListViewController : ViewController
    {
        private SimpleAction showTodayScheduleAction;
        private SimpleAction showAllScheduleAction;
        private SimpleAction sendNotificationNowAction;

        public LichhocListViewController()
        {
            InitializeComponent();

            // Chỉ định view type và target object
            TargetViewType = ViewType.ListView;
            TargetObjectType = typeof(Lichhoc);

            // Action hiển thị lịch học hôm nay
            showTodayScheduleAction = new SimpleAction(this, "ShowTodayScheduleAction", PredefinedCategory.View)
            {
                Caption = "Lịch Học Hôm Nay",
                ImageName = "Action_Calendar",
                ToolTip = "Hiển thị danh sách lịch học ngày hôm nay"
            };

            // Action hiển thị tất cả lịch học
            showAllScheduleAction = new SimpleAction(this, "ShowAllScheduleAction", PredefinedCategory.View)
            {
                Caption = "Tất Cả Lịch Học",
                ImageName = "Action_ShowAllView",
                ToolTip = "Hiển thị tất cả lịch học"
            };
            sendNotificationNowAction = new SimpleAction(this, "SendNotificationNow", PredefinedCategory.Edit)
            {
                Caption = "Gửi Thông Báo Ngay",
                ImageName = "Action_SendEmail",
                ToolTip = "Gửi email thông báo cho sinh viên về lịch học",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
                ConfirmationMessage = "Bạn có chắc chắn muốn gửi thông báo cho các lịch học đã chọn?"
            };
            // Đăng ký event handlers
            showTodayScheduleAction.Execute += ShowTodayScheduleAction_Execute;
            showAllScheduleAction.Execute += ShowAllScheduleAction_Execute;
            sendNotificationNowAction.Execute += SendNotificationNowAction_Execute;
        }
        private void SendNotificationNowAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var selectedSchedules = View.SelectedObjects.Cast<Lichhoc>().ToList();
                if (!selectedSchedules.Any())
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Vui lòng chọn ít nhất một lịch học để gửi thông báo.",
                        InformationType.Warning);
                    return;
                }

                // Khởi tạo NotificationService không cần logger
                var notificationService = new NotificationService(ObjectSpace);
                int successCount = 0;
                int failCount = 0;

                foreach (var lichHoc in selectedSchedules)
                {
                    var sinhVien = lichHoc.MaSinhVien;
                    if (sinhVien != null && !string.IsNullOrWhiteSpace(sinhVien.Email))
                    {
                        try
                        {
                            // Gửi thông báo trực tiếp
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
                            lichHoc.NotificationSent = true;
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            Application.ShowViewStrategy.ShowMessage(
                                $"Lỗi gửi thông báo cho {sinhVien.HoTen}: {ex.Message}",
                                InformationType.Error);
                        }
                    }
                    else
                    {
                        failCount++;
                        Application.ShowViewStrategy.ShowMessage(
                            $"Không thể gửi thông báo cho {sinhVien?.HoTen}: Email không hợp lệ.",
                            InformationType.Warning);
                    }
                }

                ObjectSpace.CommitChanges();

                // Hiển thị kết quả
                string message = $"Đã gửi thành công {successCount} thông báo.";
                if (failCount > 0)
                {
                    message += $"\nKhông gửi được {failCount} thông báo.";
                }
                Application.ShowViewStrategy.ShowMessage(message,
                    failCount > 0 ? InformationType.Warning : InformationType.Success);

                View.Refresh();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View is ListView listView)
            {
                try
                {
                    ShowTodaySchedule();
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException($"Lỗi khi tải dữ liệu: {ex.Message}");
                }
            }
        }

        private void ShowTodayScheduleAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                ShowTodaySchedule();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi hiển thị lịch học hôm nay: {ex.Message}");
            }
        }

        private void ShowAllScheduleAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                ShowAllSchedule();
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException($"Lỗi khi hiển thị tất cả lịch học: {ex.Message}");
            }
        }

        private void ShowTodaySchedule()
        {
            if (View is ListView listView)
            {
                var today = DateTime.Today;
                // Lọc theo ngày hiện tại
                var criteria = CriteriaOperator.Parse("NgayHoc = ?", today);
                listView.CollectionSource.Criteria["Filter"] = criteria;
                View.Refresh();
            }
        }

        private void ShowAllSchedule()
        {
            if (View is ListView listView)
            {
                // Xóa điều kiện lọc để hiển thị tất cả
                listView.CollectionSource.Criteria["Filter"] = null;
                View.Refresh();
            }
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Thực hiện các tác vụ khởi tạo bổ sung nếu cần
        }

        protected override void OnDeactivated()
        {
            // Hủy đăng ký event handlers
            showTodayScheduleAction.Execute -= ShowTodayScheduleAction_Execute;
            showAllScheduleAction.Execute -= ShowAllScheduleAction_Execute;
            base.OnDeactivated();
        }
    }
}