using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem1.Models
{
    public class Room
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "Room number is required")]
        [MaxLength(10)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required")]
        [MaxLength(50)]
        public string RoomType { get; set; } = string.Empty; // Single, Double, Suite, Deluxe

        [Required]
        [Range(1, 10, ErrorMessage = "Capacity must be between 1 and 10")]
        public int Capacity { get; set; }

        [Required]
        [Range(0.01, 100000, ErrorMessage = "Price must be greater than 0")]
        public decimal PricePerNight { get; set; }

        public bool IsAvailable { get; set; } = true;

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

