using System.Linq;
using DevExpress.Xpo;
using System;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    [DefaultClassOptions]
    public partial class Hocphi : DevExpress.Persistent.BaseImpl.BaseObject
    {
        public Hocphi(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            TinhTrangHocPhi = false;
            NgayThanhToan = DateTime.MinValue;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            UpdateSoTien();
        }

        public void UpdateSoTien()
        {
            if (MaSinhVien != null)
            {
                SoTien = Session.Query<Hocphichitiet>()
                    .Where(h => h.MaSinhvien == MaSinhVien)
                    .Sum(h => h.TongHocPhi);
            }
        }

        // Event handler for TinhTrangHocPhi property changes
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (propertyName == nameof(TinhTrangHocPhi))
            {
                if ((bool)newValue)
                {
                    NgayThanhToan = DateTime.Now;
                }
                else
                {
                    NgayThanhToan = DateTime.MinValue;
                }
            }
        }

        // Phương thức để lấy danh sách sinh viên không trùng lặp
        public static XPCollection<Sinhvien> GetDistinctSinhviens(Session session)
        {
            return new XPCollection<Sinhvien>(session, new GroupOperator(GroupOperatorType.And,
                CriteriaOperator.Parse("Oid in (?)", session.Query<Hocphi>().Select(h => h.MaSinhVien.Oid).Distinct().ToArray())));
        }
    }
}