﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    [DefaultClassOptions]
    public partial class Diemdanh : DevExpress.Persistent.BaseImpl.BaseObject
    {
        Sinhvien fMaSinhvien;
        [Association(@"DiemdanhReferencesSinhvien")]
        [DevExpress.Xpo.DisplayName(@"Mã sinh viên")]
        public Sinhvien MaSinhvien
        {
            get { return fMaSinhvien; }
            set { SetPropertyValue<Sinhvien>(nameof(MaSinhvien), ref fMaSinhvien, value); }
        }
        Lichhoc fLichhocID;
        [Association(@"DiemdanhReferencesLichhoc")]
        public Lichhoc LichhocID
        {
            get { return fLichhocID; }
            set { SetPropertyValue<Lichhoc>(nameof(LichhocID), ref fLichhocID, value); }
        }
        Monhoc fMonhocID;
        [Association(@"DiemdanhReferencesMonhoc")]
        public Monhoc MonhocID
        {
            get { return fMonhocID; }
            set { SetPropertyValue<Monhoc>(nameof(MonhocID), ref fMonhocID, value); }
        }
        bool fTrangThaiDiemDanh;
        [DevExpress.Xpo.DisplayName(@"Tình trạng điểm danh")]
        public bool TrangThaiDiemDanh
        {
            get { return fTrangThaiDiemDanh; }
            set { SetPropertyValue<bool>(nameof(TrangThaiDiemDanh), ref fTrangThaiDiemDanh, value); }
        }
        string fLyDoVangMat;
        [DevExpress.Xpo.DisplayName(@"Lý do vắng mặt")]
        public string LyDoVangMat
        {
            get { return fLyDoVangMat; }
            set { SetPropertyValue<string>(nameof(LyDoVangMat), ref fLyDoVangMat, value); }
        }
        DateTime fThoiGianDiemDanh;
        [DevExpress.Xpo.DisplayName(@"Thời gian điểm danh")]
        public DateTime ThoiGianDiemDanh
        {
            get { return fThoiGianDiemDanh; }
            set { SetPropertyValue<DateTime>(nameof(ThoiGianDiemDanh), ref fThoiGianDiemDanh, value); }
        }
    }

}
