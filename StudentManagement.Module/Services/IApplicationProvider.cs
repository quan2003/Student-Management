using DevExpress.ExpressApp;

namespace StudentManagement.Module.Services
{
    public interface IApplicationProvider
    {
        XafApplication Application { get; set; }
    }
}