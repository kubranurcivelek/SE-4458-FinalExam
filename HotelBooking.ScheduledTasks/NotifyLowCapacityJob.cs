using Quartz;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace HotelBooking.ScheduledTasks
{
    public class NotifyLowCapacityJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<NotifyLowCapacityJob> _logger;

        public NotifyLowCapacityJob(IServiceProvider serviceProvider, ILogger<NotifyLowCapacityJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Executing NotifyLowCapacityJob...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var notificationService = scope.ServiceProvider.GetRequiredService<NotificationService>();
                await notificationService.NotifyLowCapacity();
            }
        }
    }
}