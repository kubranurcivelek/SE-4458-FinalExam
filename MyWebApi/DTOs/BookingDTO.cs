namespace MyWebApi.DTOs
{

    public class BookingResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public string? UserName { get; set; }
        public string? RoomType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class BookingCreateDto
    {
        public int UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}