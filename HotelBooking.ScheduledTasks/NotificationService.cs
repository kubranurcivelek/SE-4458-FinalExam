using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;
using HotelBooking.ScheduledTasks.Interfaces;

namespace HotelBooking.ScheduledTasks
{
    public class NotificationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IQueueService _queueService;
        private readonly IEmailService _emailService;

        public NotificationService(IServiceProvider serviceProvider, IQueueService queueService, IEmailService emailService)
        {
            _serviceProvider = serviceProvider;
            _queueService = queueService;
            _emailService = emailService;
        }

        public async Task NotifyLowCapacity()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<Ap>();
                var oneMonthFromNow = DateTime.Now.AddMonths(1);
                var hotels = await context.Hotels
                    .Include(h => h.Rooms)
                    .ToListAsync();

                foreach (var hotel in hotels)
                {
                    var totalCapacity = hotel.Rooms.Sum(r => r.Capacity);
                    var reservedCapacity = await context.Bookings
                        .Where(b => b.Room.HotelId == hotel.Id && b.StartDate <= oneMonthFromNow)
                        .SumAsync(b => b.Room.Capacity);

                    if (reservedCapacity / (double)totalCapacity >= 0.8)
                    {
                        // Notify all admins
                        var admins = await context.Users
                            .Where(u => u.Role == UserRole.Admin)
                            .ToListAsync();

                        foreach (var admin in admins)
                        {
                            // Implement email notification logic here
                            SendEmailNotification(admin.Email, hotel.Name);
                        }
                    }
                }
            }
        }

        public void ProcessReservationQueue()
        {
            _queueService.ReceiveReservationMessages(async message =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var user = await context.Users.FindAsync(message.UserId);
                    var room = await context.Rooms.FindAsync(message.RoomId);

                    if (user != null && room != null)
                    {
                        var body = $"Dear {user.Username},<br/><br/>" +
                                   $"Your reservation for room {room.Type} from {message.StartDate:dd MMM yyyy} to {message.EndDate:dd MMM yyyy} has been confirmed.<br/><br/>" +
                                   "Thank you for choosing our service!";
                        _emailService.Send(user.Email, "Reservation Confirmation", body);
                    }
                }
            });
        }

        private void SendEmailNotification(string email, string hotelName)
        {
            _emailService.Send(email, "Low Capacity Alert", $"The capacity for hotel {hotelName} is below 20%");
        }
    }
}