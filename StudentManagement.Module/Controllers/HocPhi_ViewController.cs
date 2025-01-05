using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using StudentManagement.Module.BusinessObjects;
using StudentManagement.Module.BusinessObjects.StudentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudentManagement.Module.Controllers
{
    public partial class Hocphi_ViewController : ViewController
    {
        private readonly SimpleAction sendHocPhiMailAction;
        private readonly SimpleAction createHocPhiFromChiTietAction;

        public Hocphi_ViewController()
        {
            TargetObjectType = typeof(Hocphi);

            sendHocPhiMailAction = new SimpleAction(this, "SendEmailHocPhi", PredefinedCategory.Tools)
            {
                Caption = "Gửi thông báo học phí",
                ConfirmationMessage = "Xác nhận gửi email thông báo học phí cho các sinh viên đã chọn?",
                ImageName = "Action_SendEmail",
                SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            };

            createHocPhiFromChiTietAction = new SimpleAction(this, "CreateHocPhiFromChiTiet", PredefinedCategory.Tools)
            {
                Caption = "Tạo học phí từ chi tiết",
                ImageName = "Action_Import",
                ConfirmationMessage = "Xác nhận tạo học phí từ bảng chi tiết?"
            };

            Actions.Add(sendHocPhiMailAction);
            Actions.Add(createHocPhiFromChiTietAction);

            sendHocPhiMailAction.Execute += SendHocPhiMailAction_Execute;
            createHocPhiFromChiTietAction.Execute += CreateHocPhiFromChiTietAction_Execute;
        }

        private void CreateHocPhiFromChiTietAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var objectSpace = Application.CreateObjectSpace(typeof(Hocphichitiet));

                // Lấy tất cả học phí chi tiết và group theo mã sinh viên
                var chiTietGroups = objectSpace.GetObjects<Hocphichitiet>()
                    .GroupBy(ct => ct.MaSinhvien)
                    .Where(g => g.Key != null);

                int createdCount = 0;

                foreach (var group in chiTietGroups)
                {
                    var sinhVien = group.Key;
                    var tongTien = group.Sum(ct => ct.TongHocPhi);

                    // Kiểm tra xem đã có học phí cho sinh viên này chưa
                    var existingHocPhi = objectSpace.FindObject<Hocphi>(CriteriaOperator.Parse(
                        "MaSinhVien = ?", sinhVien));

                    if (existingHocPhi == null)
                    {
                        // Tạo mới học phí nếu chưa tồn tại
                        var hocPhi = objectSpace.CreateObject<Hocphi>();
                        hocPhi.MaSinhVien = sinhVien;
                        hocPhi.SoTien = tongTien;
                        hocPhi.TinhTrangHocPhi = false; // Mặc định là chưa nộp

                        // Liên kết với các chi tiết
                        foreach (var chiTiet in group)
                        {
                            chiTiet.HocPhi = hocPhi;
                        }

                        createdCount++;
                    }
                }

                objectSpace.CommitChanges();

                if (createdCount > 0)
                {
                    Application.ShowViewStrategy.ShowMessage(
                        $"Đã tạo thành công {createdCount} bản ghi học phí mới.",
                        InformationType.Success);

                    View.ObjectSpace.Refresh();
                }
                else
                {
                    Application.ShowViewStrategy.ShowMessage(
                        "Không có bản ghi học phí mới nào được tạo.",
                        InformationType.Info);
                }
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Đã xảy ra lỗi khi tạo học phí: {ex.Message}",
                    InformationType.Error);
            }
        }

        private void SendHocPhiMailAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                var selectedHocPhi = View.SelectedObjects.Cast<Hocphi>().ToList();
                var results = SendHocPhiNotifications(selectedHocPhi);
                ShowResultMessage(results);
            }
            catch (Exception ex)
            {
                Application.ShowViewStrategy.ShowMessage(
                    $"Đã xảy ra lỗi trong quá trình gửi thông báo: {ex.Message}",
                    InformationType.Error);
            }
        }

        private (int success, List<string> errors) SendHocPhiNotifications(List<Hocphi> hocPhiList)
        {
            int successCount = 0;
            var errors = new List<string>();

            var studentGroups = hocPhiList
                .GroupBy(h => h.MaSinhVien?.MaSinhVien)
                .Where(g => g.Key != null);

            foreach (var group in studentGroups)
            {
                var sinhVien = group.First().MaSinhVien;

                if (!IsValidStudent(sinhVien))
                {
                    errors.Add($"Sinh viên {sinhVien?.HoTen ?? "Không xác định"}: Thông tin email không hợp lệ");
                    continue;
                }

                try
                {
                    var unpaidFees = group.Where(h => !h.TinhTrangHocPhi).ToList();
                    var paidFees = group.Where(h => h.TinhTrangHocPhi).ToList();

                    if (unpaidFees.Any())
                    {
                        var unpaidDetails = unpaidFees
                            .SelectMany(h => h.HocPhiChiTiet)
                            .ToList();
                        SendUnpaidFeesEmail(sinhVien, unpaidDetails);
                    }

                    if (paidFees.Any())
                    {
                        SendPaidFeesEmail(sinhVien, paidFees);
                    }

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

        private void SendUnpaidFeesEmail(Sinhvien sinhVien, List<Hocphichitiet> unpaidFees)
        {
            var totalAmount = unpaidFees.Sum(h => h.TongHocPhi);
            var subject = "Thông báo học phí chưa thanh toán";

            var courseDetails = unpaidFees
                .Select(h => $@"<tr>
                    <td style='padding: 8px; border-bottom: 1px solid #ddd;'>{h.MonHocID?.TenMon ?? "Không xác định"}</td>
                    <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{h.TongHocPhi:#,0} đ</td>
                </tr>")
                .ToList();

            var content = $@"
            <div style='margin: 20px; font-family: Arial, sans-serif; line-height: 1.6;'>
                <p>Kính gửi {sinhVien.HoTen},</p>
                <p>Nhà trường thông báo các khoản học phí chưa thanh toán của bạn:</p>
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0; background-color: #f9f9f9;'>
                    <tr style='background-color: #f2f2f2;'>
                        <th style='padding: 10px; text-align: left;'>Môn học</th>
                        <th style='padding: 10px; text-align: right;'>Số tiền</th>
                    </tr>
                    {string.Join("\n", courseDetails)}
                    <tr style='background-color: #f2f2f2; font-weight: bold;'>
                        <td style='padding: 10px;'>Tổng số tiền cần thanh toán:</td>
                        <td style='padding: 10px; text-align: right;'>{totalAmount:#,0} đ</td>
                    </tr>
                </table>
                <p style='color: #d14836;'>Vui lòng hoàn tất thanh toán trong thời gian sớm nhất.</p>
                <p style='margin-top: 30px;'>Trân trọng,<br/><strong>Phòng Tài chính</strong></p>
            </div>";

            EmailHelper.SendHtmlEmail(sinhVien.Email, subject, content);
        }

        private void SendPaidFeesEmail(Sinhvien sinhVien, List<Hocphi> paidFees)
        {
            var totalAmount = paidFees.Sum(h => h.SoTien);
            var subject = "Xác nhận thanh toán học phí";

            var courseDetails = paidFees
                .Select(h => $@"<tr>
                    <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: right;'>{h.SoTien:#,0} đ</td>
                    <td style='padding: 8px; border-bottom: 1px solid #ddd; text-align: center;'>{h.NgayThanhToan:dd/MM/yyyy}</td>
                </tr>")
                .ToList();

            var content = $@"
            <div style='margin: 20px; font-family: Arial, sans-serif; line-height: 1.6;'>
                <p>Kính gửi {sinhVien.HoTen},</p>
                <p>Trường xin xác nhận bạn đã hoàn tất thanh toán học phí:</p>
                <table style='width: 100%; border-collapse: collapse; margin: 20px 0; background-color: #f9f9f9;'>
                    <tr style='background-color: #4CAF50; color: white;'>
                        <th style='padding: 10px; text-align: right;'>Số tiền</th>
                        <th style='padding: 10px; text-align: center;'>Ngày thanh toán</th>
                    </tr>
                    {string.Join("\n", courseDetails)}
                    <tr style='background-color: #e8f5e9; font-weight: bold;'>
                        <td style='padding: 10px;'>Tổng số tiền đã thanh toán:</td>
                        <td style='padding: 10px; text-align: right;'>{totalAmount:#,0} đ</td>
                    </tr>
                </table>
                <p style='color: #4CAF50;'>Cảm ơn bạn đã hoàn thành nghĩa vụ học phí.</p>
                <p style='margin-top: 30px;'>Trân trọng,<br/><strong>Phòng Tài chính</strong></p>
            </div>";

            EmailHelper.SendHtmlEmail(sinhVien.Email, subject, content);
        }

        private void ShowResultMessage((int success, List<string> errors) results)
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
    }
}