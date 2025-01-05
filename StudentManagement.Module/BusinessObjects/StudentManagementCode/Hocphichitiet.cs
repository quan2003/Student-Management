using System.Linq;
using DevExpress.Xpo;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    public partial class Hocphichitiet
    {
        public Hocphichitiet(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            LanHoc = 1; // Thiết lập lần học mặc định là 1
            TinhTrang = "Học mới"; // Trạng thái mặc định
            GenerateMaLopHocPhan(); // Gọi hàm tạo mã lớp học phần
        }

        protected override void OnSaving()
        {
            // Kiểm tra số lượng sinh viên đã tham gia lớp
            if (MonHocID != null && !IsDeleted)
            {
                int studentCount = Session.Query<Hocphichitiet>()
                    .Count(h => h.MaLopHocPhan == MaLopHocPhan);

                if (studentCount >= 70)
                {
                    throw new InvalidOperationException($"Lớp học phần {MaLopHocPhan} đã đạt giới hạn tối đa 70 sinh viên.");
                }
            }

            // Kiểm tra nếu mã lớp học phần chưa được tạo
            if (string.IsNullOrEmpty(MaLopHocPhan) && MonHocID != null)
            {
                GenerateMaLopHocPhan();
            }

            base.OnSaving();
        }

        internal decimal Sum(Func<object, object> value)
        {
            throw new NotImplementedException();
        }

        private void GenerateMaLopHocPhan()
        {
            if (MonHocID != null)
            {
                // Lấy danh sách các mã lớp học phần hiện tại liên quan đến môn học
                var existingClasses = Session.Query<Hocphichitiet>()
                    .Where(h => h.MonHocID == MonHocID)
                    .Select(h => h.MaLopHocPhan)
                    .ToList();

                // Tìm số thứ tự tiếp theo trong phạm vi từ 1 đến 10
                int nextNumber = 1;
                while (nextNumber <= 10 && existingClasses.Any(code => code == $"{MonHocID.TenMon}({nextNumber})"))
                {
                    nextNumber++;
                }

                // Nếu vượt quá giới hạn (10), ném ngoại lệ
                if (nextNumber > 10)
                {
                    throw new InvalidOperationException($"Môn học {MonHocID.TenMon} đã đạt giới hạn tối đa 10 lớp học phần.");
                }

                // Gán mã lớp học phần mới
                MaLopHocPhan = $"{MonHocID.TenMon}({nextNumber})";
            }
        }

    }
}
