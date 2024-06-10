using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models;
using MyWebApi.DTOs;
using WebAPI.Data;
using HotelBooking.ScheduledTasks.Interfaces;
using HotelBooking.ScheduledTasks.DTOs;

namespace WebAPI.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<Booking> GetBookingByIdAsync(int id);
        Task<Booking> CreateBookingAsync(BookingCreateDto bookingDto);
        Task<Booking> UpdateBookingAsync(int id, BookingCreateDto bookingDto);
        Task DeleteBookingAsync(int id);
        Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime startDate, DateTime endDate);
    }

    public class BookingService : IBookingService
    {
        private readonly AppDbContext _context;
        private readonly IQueueService _queueService;

        public BookingService(AppDbContext context, IQueueService queueService)
        {
            _queueService = queueService;
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingByIdAsync(int id)
        {
            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Booking> CreateBookingAsync(BookingCreateDto bookingDto)
        {
            var isRoomAvailable = await CheckRoomAvailabilityAsync(bookingDto.RoomId, bookingDto.StartDate, bookingDto.EndDate);
            if (!isRoomAvailable)
            {
                throw new InvalidOperationException("The room is not available for the selected dates.");
            }

            var booking = new Booking
            {
                UserId = bookingDto.UserId,
                RoomId = bookingDto.RoomId,
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate
            };

            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();

            return booking;
        }
        public async Task<Booking> UpdateBookingAsync(int id, BookingCreateDto bookingDto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return null;
            }

            booking.UserId = bookingDto.UserId;
            booking.RoomId = bookingDto.RoomId;
            booking.StartDate = bookingDto.StartDate;
            booking.EndDate = bookingDto.EndDate;

            _context.Entry(booking).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var message = new ReservationMessage
            {
                UserId = bookingDto.UserId,
                RoomId = bookingDto.RoomId,
                StartDate = bookingDto.StartDate,
                EndDate = bookingDto.EndDate
            };

            _queueService.SendReservationMessage(message);
            return booking;
        }

        public async Task DeleteBookingAsync(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckRoomAvailabilityAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            var overlappingBookings = await _context.Bookings
                .Where(b => b.RoomId == roomId &&
                            ((startDate >= b.StartDate && startDate <= b.EndDate) ||
                            (endDate >= b.StartDate && endDate <= b.EndDate) ||
                            (startDate <= b.StartDate && endDate >= b.EndDate)))
                .ToListAsync();

            return !overlappingBookings.Any();
        }
    }


}