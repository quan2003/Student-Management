using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    public partial class Sinhvien
    {
        public Sinhvien(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                NgayTao = DateTime.Now;
                Email = null;
                EnableEmailNotification = false;
            }
        }
        protected override void OnSaving()
        {
            base.OnSaving();
            NgayCapNhat = DateTime.Now;
        }
    }
}