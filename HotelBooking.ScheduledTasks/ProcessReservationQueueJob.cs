using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebAPI.Services;

namespace HotelBooking.ScheduledTasks
{
    public class ProcessReservationQueueJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProcessReservationQueueJob> _logger;

        public ProcessReservationQueueJob(IServiceProvider serviceProvider, ILogger<ProcessReservationQueueJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Executing ProcessReservationQueueJob...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                notificationService.ProcessReservationQueue();
            }
        }
    }
}