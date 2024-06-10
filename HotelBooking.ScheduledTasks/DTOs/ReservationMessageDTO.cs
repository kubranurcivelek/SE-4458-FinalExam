namespace HotelBooking.ScheduledTasks.DTOs
{
    public class ReservationMessage
    {
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}