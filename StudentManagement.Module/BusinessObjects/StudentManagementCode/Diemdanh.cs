using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace StudentManagement.Module.BusinessObjects.StudentManagement
{
    public partial class Diemdanh
    {
        public Diemdanh(Session session) : base(session) { }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(TrangThaiDiemDanh) && (bool)newValue == true)
            {
                ThoiGianDiemDanh = DateTime.Now;
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                ThoiGianDiemDanh = DateTime.Now;
            }
        }
    }
}