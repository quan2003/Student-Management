//using DevExpress.ExpressApp;
//using DevExpress.Data.Filtering;
//using StudentManagement.Module.BusinessObjects.StudentManagement;

//namespace StudentManagement.Module.Controllers
//{
//    public partial class DiemChiTietListViewController : ViewController
//    {
//        public DiemChiTietListViewController()
//        {
//            InitializeComponent();
//            TargetViewType = ViewType.ListView;
//            TargetObjectType = typeof(Diemchitiet);
//        }

//        protected override void OnActivated()
//        {
//            base.OnActivated();
//            ListView view = View as ListView;
//            if (view != null)
//            {
//                // Lấy mã sinh viên của người dùng hiện tại
//                string currentUserId = SecuritySystem.CurrentUserId.ToString();

//                // Kiểm tra nếu là admin, không lọc dữ liệu
//                if (currentUserId != "admin") // Thay "admin" bằng ID của admin nếu cần
//                {
//                    // Tạo Criteria để lọc Diemchitiet cho người dùng
//                    CriteriaOperator criteria = CriteriaOperator.Parse("MaSinhVien.MaSinhVien = ?", currentUserId);

//                    // Áp dụng Criteria vào CollectionSource của ListView
//                    view.CollectionSource.Criteria["FilterByUser"] = criteria;
//                }
//                else
//                {
//                    // Nếu là admin, không cần bộ lọc
//                    view.CollectionSource.Criteria["FilterByUser"] = null;
//                }
//            }
//        }
//    }
//}
