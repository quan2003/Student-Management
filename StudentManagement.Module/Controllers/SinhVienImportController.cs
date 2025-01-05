using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using DevExpress.Data.Filtering;
using StudentManagement.Module.BusinessObjects.StudentManagement;

public class SinhVienImportController : ObjectViewController<ListView, Sinhvien>
{
    public SinhVienImportController()
    {
        var importAction = new PopupWindowShowAction(this, "ImportExcel", "Import")
        {
            Caption = "Import Excel",
            ImageName = "Action_Export_ToXlsx",
            PaintStyle = DevExpress.ExpressApp.Templates.ActionItemPaintStyle.CaptionAndImage,
            TargetViewType = ViewType.ListView,
            SelectionDependencyType = SelectionDependencyType.Independent
        };

        importAction.CustomizePopupWindowParams += ImportAction_CustomizePopupWindowParams;
        importAction.Execute += ImportAction_Execute;
    }

    private void ImportAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(FileData));
        var fileData = objectSpace.CreateObject<FileData>();

        var detailView = Application.CreateDetailView(objectSpace, fileData);
        detailView.ViewEditMode = ViewEditMode.Edit;

        e.View = detailView;
        e.DialogController.SaveOnAccept = false;
        e.DialogController.CancelAction.Active["NeedCancel"] = false;
        e.Maximized = false;
    }

    private void ImportAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        if (((FileData)e.PopupWindowView.CurrentObject)?.Content == null)
        {
            return;
        }

        var objectSpace = Application.CreateObjectSpace(typeof(Sinhvien));
        int successCount = 0;
        int errorCount = 0;
        var errors = new List<string>();

        try
        {
            using var stream = new MemoryStream(((FileData)e.PopupWindowView.CurrentObject).Content);
            using var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            // Validate số lượng cột
            var headerRow = sheet.GetRow(0);
            if (headerRow == null || headerRow.LastCellNum < 8)
            {
                throw new UserFriendlyException("File Excel không đúng định dạng. Vui lòng kiểm tra lại!");
            }

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                try
                {
                    if (string.IsNullOrWhiteSpace(GetCellValue(row.GetCell(0)))) continue;

                    var sinhVien = objectSpace.CreateObject<Sinhvien>();

                    // Map dữ liệu
                    sinhVien.MaSinhVien = GetCellValue(row.GetCell(0))?.Trim();
                    sinhVien.HoTen = GetCellValue(row.GetCell(1))?.Trim();
                    sinhVien.Email = GetCellValue(row.GetCell(2))?.Trim();
                    sinhVien.SDT = GetCellValue(row.GetCell(3))?.Trim();
                    sinhVien.DiaChi = GetCellValue(row.GetCell(4))?.Trim();

                    var ngaySinhStr = GetCellValue(row.GetCell(5));
                    if (!string.IsNullOrWhiteSpace(ngaySinhStr))
                    {
                        if (DateTime.TryParseExact(ngaySinhStr,
                            new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy" },
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None,
                            out DateTime ngaySinh))
                        {
                            sinhVien.NgaySinh = ngaySinh;
                        }
                        else
                        {
                            throw new Exception($"Ngày sinh không đúng định dạng dd/MM/yyyy");
                        }
                    }

                    sinhVien.GioiTinh = GetCellValue(row.GetCell(6))?.Trim();
                    sinhVien.Khoa = GetCellValue(row.GetCell(7))?.Trim();
                    sinhVien.NgayTao = DateTime.Now;
                    sinhVien.NgayCapNhat = DateTime.Now;

                    // Validate dữ liệu
                    if (string.IsNullOrWhiteSpace(sinhVien.MaSinhVien))
                    {
                        throw new Exception("Mã sinh viên không được để trống");
                    }

                    if (string.IsNullOrWhiteSpace(sinhVien.HoTen))
                    {
                        throw new Exception("Họ tên không được để trống");
                    }

                    // Kiểm tra trùng mã sinh viên
                    var existingSV = objectSpace.FindObject<Sinhvien>(
                        CriteriaOperator.Parse("MaSinhVien = ?", sinhVien.MaSinhVien));

                    if (existingSV != null)
                    {
                        throw new Exception($"Mã sinh viên {sinhVien.MaSinhVien} đã tồn tại trong hệ thống");
                    }

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"Lỗi dòng {rowIndex + 1}: {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                objectSpace.CommitChanges();
            }

            View.ObjectSpace.Refresh();

            // Hiển thị kết quả
            var message = $"Import hoàn tất:\n- Thành công: {successCount} sinh viên\n- Lỗi: {errorCount} sinh viên";
            if (errors.Any())
            {
                message += "\n\nChi tiết lỗi:\n" + string.Join("\n", errors.Take(5));
                if (errors.Count > 5)
                {
                    message += "\n...và " + (errors.Count - 5) + " lỗi khác";
                }
            }

            Application.ShowViewStrategy.ShowMessage(new MessageOptions
            {
                Duration = 10000,
                Message = message,
                Type = errorCount > 0 ? InformationType.Warning : InformationType.Success
            });
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException($"Lỗi import: {ex.Message}");
        }
    }

    private string GetCellValue(ICell cell)
    {
        if (cell == null) return string.Empty;

        try
        {
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                    {
                        return cell.DateCellValue?.ToString("dd/MM/yyyy") ?? string.Empty;
                    }
                    return cell.NumericCellValue.ToString();
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Formula:
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                            {
                                return cell.DateCellValue?.ToString("dd/MM/yyyy") ?? string.Empty;
                            }
                            return cell.NumericCellValue.ToString();
                        case CellType.String:
                            return cell.StringCellValue;
                        default:
                            return string.Empty;
                    }
                default:
                    return string.Empty;
            }
        }
        catch
        {
            return string.Empty;
        }
    }
}
