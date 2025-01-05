using Quartz;
using StudentManagement.Module.Services;

namespace StudentManagement.Blazor.Server.Schedulers
{
    public class NotificationJob : IJob
    {
        private readonly IApplicationProvider applicationProvider;

        public NotificationJob(IApplicationProvider applicationProvider)
        {
            this.applicationProvider = applicationProvider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                var application = applicationProvider.Application;
                using (var objectSpace = application.CreateObjectSpace())
                {
                    var notificationService = new NotificationService(objectSpace);
                    notificationService.SendUpcomingClassNotifications();
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                throw new JobExecutionException($"Lỗi thực thi job: {ex.Message}", ex);
            }
        }
    }
}