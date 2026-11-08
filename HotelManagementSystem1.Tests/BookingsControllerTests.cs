

using HotelManagementSystem1.Controllers;
using HotelManagementSystem1.Data;
using HotelManagementSystem1.DTOs;
using HotelManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem1.Tests
{
    public class BookingsControllerTests
    {
        private HotelDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelDbContext(options);
        }

        [Fact]
        public async Task CreateBooking_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com",
                PhoneNumber = "+1234567890",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);

            var room = new Room
            {
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerNight = 50m,
                IsAvailable = true
            };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var bookingDto = new CreateBookingDto
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(1),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(3),
                SpecialRequests = "Late check-in"
            };

            // Act
            var result = await controller.CreateBooking(bookingDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedBooking = Assert.IsType<BookingDto>(createdResult.Value);
            Assert.Equal(guest.Id, returnedBooking.GuestId);
            Assert.Equal(room.Id, returnedBooking.RoomId);
            Assert.Equal(100m, returnedBooking.TotalPrice); // 2 nights × $50
            Assert.Equal("Pending", returnedBooking.Status);
        }

        [Fact]
        public async Task CreateBooking_WithOverlappingDates_ReturnsConflict()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@test.com",
                PhoneNumber = "+0987654321",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);

            var room = new Room
            {
                RoomNumber = "102",
                RoomType = "Double",
                Capacity = 2,
                PricePerNight = 80m,
                IsAvailable = true
            };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var existingBooking = new Booking
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(1),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(5),
                TotalPrice = 320m,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow
            };
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            var overlappingBookingDto = new CreateBookingDto
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(3),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(6)
            };

            // Act
            var result = await controller.CreateBooking(overlappingBookingDto);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateBooking_WithPastCheckInDate_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@test.com",
                PhoneNumber = "+1111111111",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);

            var room = new Room
            {
                RoomNumber = "103",
                RoomType = "Suite",
                Capacity = 4,
                PricePerNight = 150m,
                IsAvailable = true
            };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var bookingDto = new CreateBookingDto
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.Date.AddDays(-2),
                CheckOutDate = DateTime.UtcNow.Date.AddDays(1)
            };

            // Act
            var result = await controller.CreateBooking(bookingDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetBookingsByGuest_ReturnsGuestBookings()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest1 = new Guest { FirstName = "Guest", LastName = "One", Email = "guest1@test.com", PhoneNumber = "+1111111111", RegistrationDate = DateTime.UtcNow };
            var guest2 = new Guest { FirstName = "Guest", LastName = "Two", Email = "guest2@test.com", PhoneNumber = "+2222222222", RegistrationDate = DateTime.UtcNow };
            context.Guests.AddRange(guest1, guest2);

            var room = new Room { RoomNumber = "101", RoomType = "Single", Capacity = 1, PricePerNight = 50m, IsAvailable = true };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { GuestId = guest1.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(2), TotalPrice = 50m, Status = "Pending", BookingDate = DateTime.UtcNow },
                new Booking { GuestId = guest1.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(5), CheckOutDate = DateTime.UtcNow.AddDays(7), TotalPrice = 100m, Status = "Confirmed", BookingDate = DateTime.UtcNow },
                new Booking { GuestId = guest2.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(10), CheckOutDate = DateTime.UtcNow.AddDays(12), TotalPrice = 100m, Status = "Pending", BookingDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetBookingsByGuest(guest1.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bookings = Assert.IsAssignableFrom<IEnumerable<BookingDto>>(okResult.Value);
            Assert.Equal(2, bookings.Count());
            Assert.All(bookings, booking => Assert.Equal(guest1.Id, booking.GuestId));
        }

        [Fact]
        public async Task GetBookingsByStatus_ReturnsFilteredBookings()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest { FirstName = "Test", LastName = "Guest", Email = "test@test.com", PhoneNumber = "+1234567890", RegistrationDate = DateTime.UtcNow };
            context.Guests.Add(guest);

            var room = new Room { RoomNumber = "101", RoomType = "Single", Capacity = 1, PricePerNight = 50m, IsAvailable = true };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            context.Bookings.AddRange(
                new Booking { GuestId = guest.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(1), CheckOutDate = DateTime.UtcNow.AddDays(2), TotalPrice = 50m, Status = "Pending", BookingDate = DateTime.UtcNow },
                new Booking { GuestId = guest.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(3), CheckOutDate = DateTime.UtcNow.AddDays(4), TotalPrice = 50m, Status = "Confirmed", BookingDate = DateTime.UtcNow },
                new Booking { GuestId = guest.Id, RoomId = room.Id, CheckInDate = DateTime.UtcNow.AddDays(5), CheckOutDate = DateTime.UtcNow.AddDays(6), TotalPrice = 50m, Status = "Pending", BookingDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetBookingsByStatus("Pending");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var bookings = Assert.IsAssignableFrom<IEnumerable<BookingDto>>(okResult.Value);
            Assert.Equal(2, bookings.Count());
            Assert.All(bookings, booking => Assert.Equal("Pending", booking.Status));
        }

        [Fact]
        public async Task UpdateBookingStatus_WithValidStatus_ReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest { FirstName = "Test", LastName = "Guest", Email = "test@test.com", PhoneNumber = "+1234567890", RegistrationDate = DateTime.UtcNow };
            context.Guests.Add(guest);

            var room = new Room { RoomNumber = "101", RoomType = "Single", Capacity = 1, PricePerNight = 50m, IsAvailable = true };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(2),
                TotalPrice = 50m,
                Status = "Pending",
                BookingDate = DateTime.UtcNow
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.UpdateBookingStatus(booking.Id, "Confirmed");

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedBooking = await context.Bookings.FindAsync(booking.Id);
            Assert.Equal("Confirmed", updatedBooking.Status);
        }

        [Fact]
        public async Task CancelBooking_ChangesStatusToCancelled()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new BookingsController(context);

            var guest = new Guest
            {
                FirstName = "Test",
                LastName = "Guest",
                Email = "test@test.com",
                PhoneNumber = "+1234567890",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);

            var room = new Room
            {
                RoomNumber = "104",
                RoomType = "Suite",
                Capacity = 2,
                PricePerNight = 120m,
                IsAvailable = true
            };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(2),
                TotalPrice = 120m,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.CancelBooking(booking.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var cancelledBooking = await context.Bookings.FindAsync(booking.Id);
            Assert.Equal("Cancelled", cancelledBooking.Status);
        }
    }
}

