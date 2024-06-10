using WebAPI.Models;

namespace WebAPI.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequestDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public UserRole Role { get; set; }
    }
}