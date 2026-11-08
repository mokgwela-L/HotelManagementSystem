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
    public class RoomsController : ControllerBase
    {

        private readonly HotelDbContext _context;

        public RoomsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms()
        {
            var rooms = await _context.Rooms
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable,
                    Description = r.Description
                })
                .ToListAsync();

            return Ok(rooms);
        }

        // GET: api/rooms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {id} not found." });
            }

            var roomDto = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                Description = room.Description
            };

            return Ok(roomDto);
        }

        // GET: api/rooms/available (Filtered query)
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetAvailableRooms()
        {
            var availableRooms = await _context.Rooms
                .Where(r => r.IsAvailable == true)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable,
                    Description = r.Description
                })
                .ToListAsync();

            return Ok(availableRooms);
        }

        // GET: api/rooms/type/Suite (Filtered query by room type)
        [HttpGet("type/{roomType}")]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRoomsByType(string roomType)
        {
            var rooms = await _context.Rooms
                .Where(r => r.RoomType.ToLower() == roomType.ToLower())
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    RoomNumber = r.RoomNumber,
                    RoomType = r.RoomType,
                    Capacity = r.Capacity,
                    PricePerNight = r.PricePerNight,
                    IsAvailable = r.IsAvailable,
                    Description = r.Description
                })
                .ToListAsync();

            if (!rooms.Any())
            {
                return NotFound(new { message = $"No rooms found with type '{roomType}'." });
            }

            return Ok(rooms);
        }

        // POST: api/rooms
        [HttpPost]
        public async Task<ActionResult<RoomDto>> CreateRoom(CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Duplicate check - Room number must be unique
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.RoomNumber == dto.RoomNumber);

            if (existingRoom != null)
            {
                return Conflict(new { message = $"Room with number '{dto.RoomNumber}' already exists." });
            }

            var room = new Room
            {
                RoomNumber = dto.RoomNumber,
                RoomType = dto.RoomType,
                Capacity = dto.Capacity,
                PricePerNight = dto.PricePerNight,
                IsAvailable = true,
                Description = dto.Description
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            var roomDto = new RoomDto
            {
                Id = room.Id,
                RoomNumber = room.RoomNumber,
                RoomType = room.RoomType,
                Capacity = room.Capacity,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                Description = room.Description
            };

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, roomDto);
        }

        // PUT: api/rooms/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {id} not found." });
            }

            // Check for duplicate room number (excluding current room)
            var duplicateRoom = await _context.Rooms
                .AnyAsync(r => r.RoomNumber == dto.RoomNumber && r.Id != id);

            if (duplicateRoom)
            {
                return Conflict(new { message = $"Another room with number '{dto.RoomNumber}' already exists." });
            }

            room.RoomNumber = dto.RoomNumber;
            room.RoomType = dto.RoomType;
            room.Capacity = dto.Capacity;
            room.PricePerNight = dto.PricePerNight;
            room.Description = dto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/rooms/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.Include(r => r.Bookings)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {id} not found." });
            }

            // Check if room has active bookings
            var hasActiveBookings = room.Bookings
                .Any(b => b.Status == "Pending" || b.Status == "Confirmed");

            if (hasActiveBookings)
            {
                return BadRequest(new { message = "Cannot delete room with active bookings." });
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/rooms/5/availability
        [HttpPatch("{id}/availability")]
        public async Task<IActionResult> UpdateRoomAvailability(int id, [FromBody] bool isAvailable)
        {
            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound(new { message = $"Room with ID {id} not found." });
            }

            room.IsAvailable = isAvailable;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

