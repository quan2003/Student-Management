using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace StudentManagement.Module.BusinessObjects.StudentManagement
{

    public partial class Monhoc
    {
        public Monhoc(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
