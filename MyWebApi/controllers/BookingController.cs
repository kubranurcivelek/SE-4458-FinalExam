using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.ScheduledTasks.DTOs;
using HotelBooking.ScheduledTasks.Interfaces;
using Microsoft.AspNetCore.Mvc;
using MyWebApi.DTOs;
using WebAPI.Models;
using WebAPI.Services;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IQueueService _queueService;
        public BookingController(IBookingService bookingService, IQueueService queueService)
        {
            _queueService = queueService;
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var bookings = await _bookingService.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return booking;
        }

        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(BookingCreateDto bookingDto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(bookingDto);

                var message = new ReservationMessage
                {
                    UserId = bookingDto.UserId,
                    RoomId = bookingDto.RoomId,
                    StartDate = bookingDto.StartDate,
                    EndDate = bookingDto.EndDate
                };

                _queueService.SendReservationMessage(message);
                return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, booking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(int id, BookingCreateDto bookingDto)
        {
            var booking = await _bookingService.UpdateBookingAsync(id, bookingDto);
            if (booking == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            await _bookingService.DeleteBookingAsync(id);
            return NoContent();
        }
    }
}