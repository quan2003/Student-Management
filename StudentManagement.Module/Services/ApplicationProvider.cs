using DevExpress.ExpressApp;

namespace StudentManagement.Module.Services
{
    public class ApplicationProvider : IApplicationProvider
    {
        private XafApplication? application;

        public XafApplication Application
        {
            get => application ?? throw new InvalidOperationException("Application not initialized");
            set => application = value;
        }
    }
}