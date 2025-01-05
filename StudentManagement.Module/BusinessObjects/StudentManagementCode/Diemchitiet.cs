using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Validation;
using System.ComponentModel;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    [RuleCriteria("DiemChuyenCanRange", DefaultContexts.Save,
        "DiemChuyenCan >= 0 && DiemChuyenCan <= 10",
        "Điểm chuyên cần phải nằm trong khoảng từ 0 đến 10")]
    [RuleCriteria("DiemBaiTapRange", DefaultContexts.Save,
        "DiemBaiTap >= 0 && DiemBaiTap <= 10",
        "Điểm bài tập phải nằm trong khoảng từ 0 đến 10")]
    [RuleCriteria("DiemGiuaKyRange", DefaultContexts.Save,
        "DiemGiuaKy >= 0 && DiemGiuaKy <= 10",
        "Điểm giữa kỳ phải nằm trong khoảng từ 0 đến 10")]
    [RuleCriteria("DiemCuoiKyRange", DefaultContexts.Save,
        "DiemCuoiKy >= 0 && DiemCuoiKy <= 10",
        "Điểm cuối kỳ phải nằm trong khoảng từ 0 đến 10")]
    public partial class Diemchitiet : DevExpress.Persistent.BaseImpl.BaseObject
    {
        public Diemchitiet(Session session) : base(session)
        {
            // Khởi tạo giá trị mặc định
            fDiemChuyenCan = 0;
            fDiemBaiTap = 0;
            fDiemGiuaKy = 0;
            fDiemCuoiKy = 0;
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        protected override void OnSaving()
        {
            base.OnSaving();
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
        }

        protected override void OnDeleting()
        {
            if (Session.IsNewObject(this))
            {
                base.OnDeleting();
                return;
            }

            base.OnDeleting();
        }

    }
}