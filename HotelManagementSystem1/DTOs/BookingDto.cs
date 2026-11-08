using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem1.DTOs
{
    public class CreateBookingDto
    {
        [Required]
        public int GuestId { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public string? SpecialRequests { get; set; }
    }

    public class BookingDto
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; } = string.Empty;
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public string? SpecialRequests { get; set; }
    }
}
