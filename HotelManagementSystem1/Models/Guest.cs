using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem1.Models
{
    public class Guest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Address { get; set; }

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        // NEW: Role field
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Guest"; // Default role

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
