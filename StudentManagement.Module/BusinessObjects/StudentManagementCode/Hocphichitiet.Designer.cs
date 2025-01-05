// Updated Hocphichitiet Class
using System;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    [DefaultClassOptions]
    public partial class Hocphichitiet : DevExpress.Persistent.BaseImpl.BaseObject
    {
        Sinhvien fMaSinhvien;
        [Association(@"HocphichitietReferencesSinhvien")]
        public Sinhvien MaSinhvien
        {
            get { return fMaSinhvien; }
            set { SetPropertyValue<Sinhvien>(nameof(MaSinhvien), ref fMaSinhvien, value); }
        }

        Monhoc fMonHocID;
        [Association(@"HocphichitietReferencesMonhoc")]
        public Monhoc MonHocID
        {
            get { return fMonHocID; }
            set { SetPropertyValue<Monhoc>(nameof(MonHocID), ref fMonHocID, value); }
        }

        string fMaLopHocPhan;
        public string MaLopHocPhan
        {
            get { return fMaLopHocPhan; }
            set { SetPropertyValue<string>(nameof(MaLopHocPhan), ref fMaLopHocPhan, value); }
        }

        int fLanHoc;
        public int LanHoc
        {
            get { return fLanHoc; }
            set { SetPropertyValue<int>(nameof(LanHoc), ref fLanHoc, value); }
        }

        int fSoTinChi;
        public int SoTinChi
        {
            get { return fSoTinChi; }
            set { SetPropertyValue<int>(nameof(SoTinChi), ref fSoTinChi, value); }
        }
        [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "### ### ### ###")]
        [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "### ### ### ###")]
        [PersistentAlias("Iif(MonHocID is not null, SoTinChi * MonHocID.HocPhiTheoTichChi, 0)")]
        public decimal TongHocPhi
        {
            get
            {
                if (EvaluateAlias(nameof(TongHocPhi)) is decimal result)
                {
                    return result;
                }
                else
                {
                    return 0;
                }
            }
        }

        string fTinhTrang;
        public string TinhTrang
        {
            get { return fTinhTrang; }
            set { SetPropertyValue<string>(nameof(TinhTrang), ref fTinhTrang, value); }
        }

        Hocphi fHocPhi;
        [Association(@"HocphiReferencesHocphichitiet")]
        public Hocphi HocPhi
        {
            get { return fHocPhi; }
            set { SetPropertyValue<Hocphi>(nameof(HocPhi), ref fHocPhi, value); }
        }
    }
}
