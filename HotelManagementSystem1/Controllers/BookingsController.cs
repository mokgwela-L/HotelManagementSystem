using HotelManagementSystem1.Data;
using HotelManagementSystem1.DTOs;
using HotelManagementSystem1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {

        private readonly HotelDbContext _context;

        public BookingsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetAllBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    GuestId = b.GuestId,
                    GuestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
                    RoomId = b.RoomId,
                    RoomNumber = b.Room.RoomNumber,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    BookingDate = b.BookingDate,
                    SpecialRequests = b.SpecialRequests
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDto>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found." });
            }

            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                GuestId = booking.GuestId,
                GuestName = $"{booking.Guest.FirstName} {booking.Guest.LastName}",
                RoomId = booking.RoomId,
                RoomNumber = booking.Room.RoomNumber,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate,
                SpecialRequests = booking.SpecialRequests
            };

            return Ok(bookingDto);
        }

        // GET: api/bookings/guest/5 (Filtered query)
        [HttpGet("guest/{guestId}")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByGuest(int guestId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Where(b => b.GuestId == guestId)
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    GuestId = b.GuestId,
                    GuestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
                    RoomId = b.RoomId,
                    RoomNumber = b.Room.RoomNumber,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    BookingDate = b.BookingDate,
                    SpecialRequests = b.SpecialRequests
                })
                .ToListAsync();

            if (!bookings.Any())
            {
                return NotFound(new { message = $"No bookings found for guest ID {guestId}." });
            }

            return Ok(bookings);
        }

        // GET: api/bookings/status/Confirmed (Filtered query)
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<BookingDto>>> GetBookingsByStatus(string status)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .Where(b => b.Status.ToLower() == status.ToLower())
                .Select(b => new BookingDto
                {
                    Id = b.Id,
                    GuestId = b.GuestId,
                    GuestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
                    RoomId = b.RoomId,
                    RoomNumber = b.Room.RoomNumber,
                    CheckInDate = b.CheckInDate,
                    CheckOutDate = b.CheckOutDate,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    BookingDate = b.BookingDate,
                    SpecialRequests = b.SpecialRequests
                })
                .ToListAsync();

            if (!bookings.Any())
            {
                return NotFound(new { message = $"No bookings found with status '{status}'." });
            }

            return Ok(bookings);
        }

        // POST: api/bookings
        [HttpPost]
        public async Task<ActionResult<BookingDto>> CreateBooking(CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate dates
            if (dto.CheckInDate >= dto.CheckOutDate)
            {
                return BadRequest(new { message = "Check-out date must be after check-in date." });
            }

            if (dto.CheckInDate < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Check-in date cannot be in the past." });
            }

            // Check if guest exists
            var guestExists = await _context.Guests.AnyAsync(g => g.Id == dto.GuestId);
            if (!guestExists)
            {
                return NotFound(new { message = $"Guest with ID {dto.GuestId} not found." });
            }

            // Check if room exists and is available
            var room = await _context.Rooms.FindAsync(dto.RoomId);
            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {dto.RoomId} not found." });
            }

            if (!room.IsAvailable)
            {
                return BadRequest(new { message = "Room is not available." });
            }

            // Check for overlapping bookings
            var hasOverlap = await _context.Bookings
                .AnyAsync(b => b.RoomId == dto.RoomId &&
                              b.Status != "Cancelled" &&
                              ((dto.CheckInDate >= b.CheckInDate && dto.CheckInDate < b.CheckOutDate) ||
                               (dto.CheckOutDate > b.CheckInDate && dto.CheckOutDate <= b.CheckOutDate) ||
                               (dto.CheckInDate <= b.CheckInDate && dto.CheckOutDate >= b.CheckOutDate)));

            if (hasOverlap)
            {
                return Conflict(new { message = "Room is already booked for the selected dates." });
            }

            // Calculate total price
            var numberOfNights = (dto.CheckOutDate - dto.CheckInDate).Days;
            var totalPrice = numberOfNights * room.PricePerNight;

            var booking = new Booking
            {
                GuestId = dto.GuestId,
                RoomId = dto.RoomId,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                TotalPrice = totalPrice,
                Status = "Pending",
                BookingDate = DateTime.UtcNow,
                SpecialRequests = dto.SpecialRequests
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(booking).Reference(b => b.Guest).LoadAsync();
            await _context.Entry(booking).Reference(b => b.Room).LoadAsync();

            var bookingDto = new BookingDto
            {
                Id = booking.Id,
                GuestId = booking.GuestId,
                GuestName = $"{booking.Guest.FirstName} {booking.Guest.LastName}",
                RoomId = booking.RoomId,
                RoomNumber = booking.Room.RoomNumber,
                CheckInDate = booking.CheckInDate,
                CheckOutDate = booking.CheckOutDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status,
                BookingDate = booking.BookingDate,
                SpecialRequests = booking.SpecialRequests
            };

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, bookingDto);
        }

        // PUT: api/bookings/5/status
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
        {
            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed" };

            if (!validStatuses.Contains(status))
            {
                return BadRequest(new { message = "Invalid status. Valid values: Pending, Confirmed, Cancelled, Completed" });
            }

            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found." });
            }

            booking.Status = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound(new { message = $"Booking with ID {id} not found." });
            }

            // Instead of deleting, we'll cancel it
            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

