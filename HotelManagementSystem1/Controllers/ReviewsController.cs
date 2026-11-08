using HotelManagementSystem1.Data;
using HotelManagementSystem1.DTOs;
using HotelManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly HotelDbContext _context;

        public ReviewsController(HotelDbContext context)
        {
            _context = context;
        }

        // GET: api/reviews - Get all reviews (Receptionist only)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.Guest)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    GuestId = r.GuestId,
                    GuestName = $"{r.Guest.FirstName} {r.Guest.LastName}",
                    GuestEmail = r.Guest.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/guest/{guestId} - Get reviews by specific guest
        [HttpGet("guest/{guestId}")]
        public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByGuest(int guestId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.Guest)
                .Where(r => r.GuestId == guestId)
                .OrderByDescending(r => r.ReviewDate)
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    GuestId = r.GuestId,
                    GuestName = $"{r.Guest.FirstName} {r.Guest.LastName}",
                    GuestEmail = r.Guest.Email,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return Ok(reviews);
        }

        // GET: api/reviews/stats - Get rating statistics
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetReviewStats()
        {
            var reviews = await _context.Reviews.ToListAsync();

            if (!reviews.Any())
            {
                return Ok(new
                {
                    totalReviews = 0,
                    averageRating = 0,
                    fiveStars = 0,
                    fourStars = 0,
                    threeStars = 0,
                    twoStars = 0,
                    oneStar = 0
                });
            }

            var stats = new
            {
                totalReviews = reviews.Count,
                averageRating = Math.Round(reviews.Average(r => r.Rating), 1),
                fiveStars = reviews.Count(r => r.Rating == 5),
                fourStars = reviews.Count(r => r.Rating == 4),
                threeStars = reviews.Count(r => r.Rating == 3),
                twoStars = reviews.Count(r => r.Rating == 2),
                oneStar = reviews.Count(r => r.Rating == 1)
            };

            return Ok(stats);
        }

        // POST: api/reviews - Create a new review
        [HttpPost]
        public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if guest exists
            var guestExists = await _context.Guests.AnyAsync(g => g.Id == dto.GuestId);
            if (!guestExists)
            {
                return NotFound(new { message = "Guest not found." });
            }

            // Check if guest already submitted a review
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.GuestId == dto.GuestId);

            if (existingReview != null)
            {
                return Conflict(new { message = "You have already submitted a review. You can only submit one review." });
            }

            var review = new Review
            {
                GuestId = dto.GuestId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                ReviewDate = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Load guest for response
            await _context.Entry(review).Reference(r => r.Guest).LoadAsync();

            var reviewDto = new ReviewDto
            {
                Id = review.Id,
                GuestId = review.GuestId,
                GuestName = $"{review.Guest.FirstName} {review.Guest.LastName}",
                GuestEmail = review.Guest.Email,
                Rating = review.Rating,
                Comment = review.Comment,
                ReviewDate = review.ReviewDate
            };

            return CreatedAtAction(nameof(GetReviewsByGuest), new { guestId = review.GuestId }, reviewDto);
        }

        // DELETE: api/reviews/{id} - Delete a review
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);

            if (review == null)
            {
                return NotFound(new { message = "Review not found." });
            }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}