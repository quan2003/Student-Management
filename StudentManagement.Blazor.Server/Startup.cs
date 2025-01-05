using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor.Services;
using DevExpress.Persistent.Base;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Server.Circuits;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp;
using Quartz;
// Chỉ sử dụng IApplicationProvider từ Module
using StudentManagement.Module.Services;
using StudentManagement.Blazor.Server.Schedulers;
using StudentManagement.Blazor.Server.Services;

namespace StudentManagement.Blazor.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(Microsoft.AspNetCore.SignalR.HubConnectionHandler<>), typeof(ProxyHubConnectionHandler<>));
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddHttpContextAccessor();
            services.AddScoped<CircuitHandler, CircuitHandlerProxy>();

            // Đăng ký ApplicationProvider từ Module
            var applicationProvider = new StudentManagement.Module.Services.ApplicationProvider();
            services.AddSingleton<IApplicationProvider>(applicationProvider);

            // Cấu hình Quartz.NET
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey("notificationJob", "defaultGroup");
                q.AddJob<NotificationJob>(opts => opts
                    .WithIdentity(jobKey)
                    .StoreDurably());

                // Sửa lại cấu hình trigger không cần Build()
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("notificationTrigger", "defaultGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(30)  // Chạy mỗi 30 giây
                        .RepeatForever()));

                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
            });

            // Cấu hình Quartz Hosted Service
            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
                options.AwaitApplicationStarted = true;
            });

            // Đăng ký SchedulerService
            services.AddHostedService<SchedulerService>();

            // Cấu hình XAF Application
            services.AddXaf(Configuration, builder =>
            {
                builder.UseApplication<StudentManagementBlazorApplication>();
                builder.Modules
                    .AddConditionalAppearance()
                    .AddNotifications()
                    .AddReports(options =>
                    {
                        options.EnableInplaceReports = true;
                        options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
                        options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
                    })
                    .AddValidation(options =>
                    {
                        options.AllowValidationDetailsAccess = false;
                    })
                    .Add<StudentManagement.Module.StudentManagementModule>()
                    .Add<StudentManagementBlazorModule>();

                builder.ObjectSpaceProviders
                    .AddSecuredXpo((serviceProvider, options) =>
                    {
                        string connectionString = null;
                        if (Configuration.GetConnectionString("ConnectionString") != null)
                        {
                            connectionString = Configuration.GetConnectionString("ConnectionString");
                        }
#if EASYTEST
                        if (Configuration.GetConnectionString("EasyTestConnectionString") != null)
                        {
                            connectionString = Configuration.GetConnectionString("EasyTestConnectionString");
                        }
#endif
                        ArgumentNullException.ThrowIfNull(connectionString);
                        options.ConnectionString = connectionString;
                        options.ThreadSafe = true;
                        options.UseSharedDataStoreProvider = true;
                    })
                    .AddNonPersistent();

                builder.Security
                    .UseIntegratedMode(options =>
                    {
                        options.RoleType = typeof(PermissionPolicyRole);
                        options.UserType = typeof(StudentManagement.Module.BusinessObjects.ApplicationUser);
                        options.UserLoginInfoType = typeof(StudentManagement.Module.BusinessObjects.ApplicationUserLoginInfo);
                        options.UseXpoPermissionsCaching();
                        options.Events.OnSecurityStrategyCreated += securityStrategy =>
                        {
                            ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                        };
                    })
                    .AddPasswordAuthentication(options =>
                    {
                        options.IsSupportChangePassword = true;
                    });
            });

            var authentication = services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
            authentication.AddCookie(options =>
            {
                options.LoginPath = "/LoginPage";
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseXaf();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapXafEndpoints();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllers();
            });
        }
    }
}