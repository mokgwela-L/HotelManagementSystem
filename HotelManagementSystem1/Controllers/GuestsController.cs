using HotelManagementSystem1.Data;
using HotelManagementSystem1.DTOs;
using HotelManagementSystem1.Models;
using HotelManagementSystem1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public GuestsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/guests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<GuestDto>>> GetAllGuests()
        {
            var guests = await _context.Guests
                .Select(g => new GuestDto
                {
                    Id = g.Id,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    Email = g.Email,
                    PhoneNumber = g.PhoneNumber,
                    Address = g.Address,
                    Role = g.Role,
                    RegistrationDate = g.RegistrationDate
                })
                .ToListAsync();

            return Ok(guests);
        }

        // GET: api/guests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GuestDto>> GetGuest(int id)
        {
            var guest = await _context.Guests.FindAsync(id);

            if (guest == null)
            {
                return NotFound(new { message = $"Guest with ID {id} not found." });
            }

            var guestDto = new GuestDto
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                Address = guest.Address,
                Role = guest.Role,
                RegistrationDate = guest.RegistrationDate
            };

            return Ok(guestDto);
        }

        // POST: api/guests/register
        [HttpPost("register")]
        public async Task<ActionResult<GuestDto>> RegisterGuest(CreateGuestDto dto)
        {
            // Validation is automatic via Data Annotations
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Duplicate check - Email must be unique
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == dto.Email);

            if (existingGuest != null)
            {
                return Conflict(new { message = "A guest with this email already exists." });
            }

            // Hash the password
            var passwordHash = PasswordService.HashPassword(dto.Password);

            // Determine role (default to Guest if not specified)
            var role = string.IsNullOrEmpty(dto.Role) ? "Guest" : dto.Role;

            // Validate role
            var validRoles = new[] { "Guest", "Receptionist", "Admin" };
            if (!validRoles.Contains(role))
            {
                return BadRequest(new { message = "Invalid role. Must be Guest, Receptionist, or Admin." });
            }

            var guest = new Guest
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                PasswordHash = passwordHash,
                Role = role,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Guests.Add(guest);
            await _context.SaveChangesAsync();

            var guestDto = new GuestDto
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                Address = guest.Address,
                Role = guest.Role,
                RegistrationDate = guest.RegistrationDate
            };

            return CreatedAtAction(nameof(GetGuest), new { id = guest.Id }, guestDto);
        }

        // POST: api/guests/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find guest by email
            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == dto.Email);

            if (guest == null)
            {
                return Ok(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            // Verify password
            if (!PasswordService.VerifyPassword(dto.Password, guest.PasswordHash))
            {
                return Ok(new LoginResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password."
                });
            }

            // Login successful - Return guest info with role
            var guestDto = new GuestDto
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                Address = guest.Address,
                Role = guest.Role,
                RegistrationDate = guest.RegistrationDate
            };

            return Ok(new LoginResponseDto
            {
                Success = true,
                Message = $"Login successful! Welcome {guest.FirstName}.",
                Guest = guestDto
            });
        }

        // Alternative route: POST: api/register (multiple routing)
        [HttpPost]
        [Route("/api/register")]
        public async Task<ActionResult<GuestDto>> Register(CreateGuestDto dto)
        {
            return await RegisterGuest(dto);
        }

        // PUT: api/guests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGuest(int id, CreateGuestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var guest = await _context.Guests.FindAsync(id);

            if (guest == null)
            {
                return NotFound(new { message = $"Guest with ID {id} not found." });
            }

            // Check for duplicate email (excluding current guest)
            var duplicateEmail = await _context.Guests
                .AnyAsync(g => g.Email == dto.Email && g.Id != id);

            if (duplicateEmail)
            {
                return Conflict(new { message = "Another guest with this email already exists." });
            }

            guest.FirstName = dto.FirstName;
            guest.LastName = dto.LastName;
            guest.Email = dto.Email;
            guest.PhoneNumber = dto.PhoneNumber;
            guest.Address = dto.Address;

            // Update password if provided
            if (!string.IsNullOrEmpty(dto.Password))
            {
                guest.PasswordHash = PasswordService.HashPassword(dto.Password);
            }

            // Update role if provided and valid
            if (!string.IsNullOrEmpty(dto.Role))
            {
                var validRoles = new[] { "Guest", "Receptionist", "Admin" };
                if (validRoles.Contains(dto.Role))
                {
                    guest.Role = dto.Role;
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/guests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (guest == null)
            {
                return NotFound(new { message = $"Guest with ID {id} not found." });
            }

            // Check if guest has active bookings
            var hasActiveBookings = guest.Bookings
                .Any(b => b.Status == "Pending" || b.Status == "Confirmed");

            if (hasActiveBookings)
            {
                return BadRequest(new { message = "Cannot delete guest with active bookings." });
            }

            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/guests/search?email=test@example.com
        [HttpGet("search")]
        public async Task<ActionResult<GuestDto>> SearchByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "Email parameter is required." });
            }

            var guest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == email);

            if (guest == null)
            {
                return NotFound(new { message = "Guest not found." });
            }

            var guestDto = new GuestDto
            {
                Id = guest.Id,
                FirstName = guest.FirstName,
                LastName = guest.LastName,
                Email = guest.Email,
                PhoneNumber = guest.PhoneNumber,
                Address = guest.Address,
                Role = guest.Role,
                RegistrationDate = guest.RegistrationDate
            };

            return Ok(guestDto);
        }

        // GET: api/guests/role/{role} - NEW: Get guests by role
        [HttpGet("role/{role}")]
        public async Task<ActionResult<IEnumerable<GuestDto>>> GetGuestsByRole(string role)
        {
            var validRoles = new[] { "Guest", "Receptionist", "Admin" };
            if (!validRoles.Contains(role))
            {
                return BadRequest(new { message = "Invalid role. Must be Guest, Receptionist, or Admin." });
            }

            var guests = await _context.Guests
                .Where(g => g.Role == role)
                .Select(g => new GuestDto
                {
                    Id = g.Id,
                    FirstName = g.FirstName,
                    LastName = g.LastName,
                    Email = g.Email,
                    PhoneNumber = g.PhoneNumber,
                    Address = g.Address,
                    Role = g.Role,
                    RegistrationDate = g.RegistrationDate
                })
                .ToListAsync();

            return Ok(guests);
        }
    }
}
