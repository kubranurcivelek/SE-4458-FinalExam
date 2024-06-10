using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Services
{
    public interface IUserService
    {
        Task<User> LoginAsync(string username, string password);
        Task<User> RegisterAsync(User user);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task UpdateUserAsync(int id, User user);
        Task DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            return user;
        }

        public async Task<User> RegisterAsync(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == user.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User already exists");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task UpdateUserAsync(int id, User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}