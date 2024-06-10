using System.Collections.Generic;

namespace WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public UserRole CustomRole { get; set; } = UserRole.NormalUser;
        public ICollection<Booking>? Bookings { get; set; }
    }
}