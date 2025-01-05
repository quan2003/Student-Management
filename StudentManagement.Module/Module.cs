using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.FileAttachments.Blazor;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Blazor;
using Microsoft.Extensions.DependencyInjection;
using StudentManagement.Module.Services;

namespace StudentManagement.Module
{
    public sealed class StudentManagementModule : ModuleBase
    {

        public StudentManagementModule()
        {
            RequiredModuleTypes.Add(typeof(FileAttachmentsBlazorModule));
            RequiredModuleTypes.Add(typeof(SystemModule));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifference));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifferenceAspect));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Notifications.NotificationsModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ReportsV2.ReportsModuleV2));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
        }

        public override void Setup(XafApplication application)
        {   
            base.Setup(application);
            
            if (application.ServiceProvider != null)
            {
                try
                {
                    var provider = application.ServiceProvider.GetRequiredService<IApplicationProvider>();
                    provider.Application = application;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        "Failed to initialize ApplicationProvider. Make sure it's registered in Startup.cs", ex);
                }
            }
        }

        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
            return new ModuleUpdater[] { updater };
        }
    }
}