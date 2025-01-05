using DevExpress.ExpressApp;
using Microsoft.Extensions.Logging;
using Quartz;
using StudentManagement.Module.Services;

namespace StudentManagement.Blazor.Server.Schedulers
{
    public class SchedulerService : IHostedService
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly ILogger<SchedulerService> logger;
        private readonly IApplicationProvider applicationProvider;
        private IScheduler? scheduler;

        public SchedulerService(
            ISchedulerFactory schedulerFactory,
            IApplicationProvider applicationProvider,
            ILogger<SchedulerService> logger)
        {
            this.schedulerFactory = schedulerFactory;
            this.applicationProvider = applicationProvider;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                scheduler = await schedulerFactory.GetScheduler(cancellationToken);

                var jobDetail = JobBuilder.Create<NotificationJob>()
                    .WithIdentity("notificationJob", "defaultGroup")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("notificationTrigger", "defaultGroup")
                    .StartNow()
                    .WithCronSchedule("0 * * * * ?")
                    .Build();

                if (!await scheduler.CheckExists(jobDetail.Key, cancellationToken))
                {
                    await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken);
                }

                await scheduler.Start(cancellationToken);
                logger.LogInformation("Scheduler started successfully at: {time}", DateTime.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting scheduler at: {time}", DateTime.Now);
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (scheduler != null && !scheduler.IsShutdown)
            {
                try
                {
                    await scheduler.Shutdown(cancellationToken);
                    logger.LogInformation("Scheduler stopped successfully at: {time}", DateTime.Now);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error stopping scheduler at: {time}", DateTime.Now);
                    throw;
                }
            }
        }
    }
}