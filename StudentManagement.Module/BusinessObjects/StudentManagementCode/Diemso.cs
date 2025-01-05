using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace StudentManagement.Module.BusinessObjects.StudentManagement
{

    public partial class Diemso
    {
        public Diemso(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
