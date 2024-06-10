using HotelBooking.ScheduledTasks.DTOs;

namespace HotelBooking.ScheduledTasks.Interfaces
{
    public interface IQueueService
    {
        void SendReservationMessage(ReservationMessage message);
        void ReceiveReservationMessages(Action<ReservationMessage> processMessage);
    }
}