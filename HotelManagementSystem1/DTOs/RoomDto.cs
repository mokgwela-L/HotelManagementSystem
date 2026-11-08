using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem1.DTOs
{
    public class CreateRoomDto
    {
        [Required(ErrorMessage = "Room number is required")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Room type is required")]
        public string RoomType { get; set; } = string.Empty;

        [Required]
        [Range(1, 10)]
        public int Capacity { get; set; }

        [Required]
        [Range(0.01, 100000)]
        public decimal PricePerNight { get; set; }

        public string? Description { get; set; }
    }

    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public string? Description { get; set; }
    }
}
