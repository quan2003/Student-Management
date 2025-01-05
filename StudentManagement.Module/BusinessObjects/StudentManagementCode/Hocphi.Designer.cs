// Updated Hocphi Class
using System;
using System.Linq;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
 
    public partial class Hocphi : DevExpress.Persistent.BaseImpl.BaseObject
    {
        Sinhvien fMaSinhVien;
        [Association(@"HocphiReferencesSinhvien")]
        public Sinhvien MaSinhVien
        {
            get { return fMaSinhVien; }
            set { SetPropertyValue<Sinhvien>(nameof(MaSinhVien), ref fMaSinhVien, value); }
        }

        decimal fSoTien;
        [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "### ### ### ###")]
        [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "### ### ### ###")]
        public decimal SoTien

        {
            get { return fSoTien; }
            set { SetPropertyValue<decimal>(nameof(SoTien), ref fSoTien, value); }
        }

        DateTime fNgayThanhToan;
        public DateTime NgayThanhToan
        {
            get { return fNgayThanhToan; }
            set { SetPropertyValue<DateTime>(nameof(NgayThanhToan), ref fNgayThanhToan, value); }
        }

        bool fTinhTrangHocPhi;
        public bool TinhTrangHocPhi
        {
            get { return fTinhTrangHocPhi; }
            set { SetPropertyValue<bool>(nameof(TinhTrangHocPhi), ref fTinhTrangHocPhi, value); }
        }

        [Association(@"HocphiReferencesHocphichitiet")]
        public XPCollection<Hocphichitiet> HocPhiChiTiet
        {
            get { return GetCollection<Hocphichitiet>(nameof(HocPhiChiTiet)); }
        }
    }
}
