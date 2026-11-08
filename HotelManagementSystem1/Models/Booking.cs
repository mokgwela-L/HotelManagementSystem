using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem1.Models
{
    public class Booking
    {

        public int Id { get; set; }

        [Required]
        public int GuestId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }

        // Navigation properties
        public Guest Guest { get; set; } = null!;
        public Room Room { get; set; } = null!;
    }
}

