using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotelManagementSystem1.Controllers;
using HotelManagementSystem1.Data;
using HotelManagementSystem1.DTOs;
using HotelManagementSystem1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HotelManagement.Tests
{
    public class GuestsControllerTests
    {
        private HotelDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelDbContext(options);
        }

        [Fact]
        public async Task RegisterGuest_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var newGuest = new CreateGuestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@test.com",
                PhoneNumber = "+1234567890",
                Address = "123 Test Street"
            };

            // Act
            var result = await controller.RegisterGuest(newGuest);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedGuest = Assert.IsType<GuestDto>(createdResult.Value);
            Assert.Equal("John", returnedGuest.FirstName);
            Assert.Equal("john.doe@test.com", returnedGuest.Email);
        }

        [Fact]
        public async Task RegisterGuest_WithDuplicateEmail_ReturnsConflict()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var existingGuest = new Guest
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "duplicate@test.com",
                PhoneNumber = "+0987654321",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(existingGuest);
            await context.SaveChangesAsync();

            var newGuest = new CreateGuestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "duplicate@test.com",
                PhoneNumber = "+1234567890",
                Address = "123 Test Street"
            };

            // Act
            var result = await controller.RegisterGuest(newGuest);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetGuest_WithValidId_ReturnsGuest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var guest = new Guest
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "+1111111111",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetGuest(guest.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedGuest = Assert.IsType<GuestDto>(okResult.Value);
            Assert.Equal("Test", returnedGuest.FirstName);
            Assert.Equal("test@example.com", returnedGuest.Email);
        }

        [Fact]
        public async Task GetGuest_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            // Act
            var result = await controller.GetGuest(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAllGuests_ReturnsAllGuests()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            context.Guests.AddRange(
                new Guest { FirstName = "Guest1", LastName = "One", Email = "guest1@test.com", PhoneNumber = "+1111111111", RegistrationDate = DateTime.UtcNow },
                new Guest { FirstName = "Guest2", LastName = "Two", Email = "guest2@test.com", PhoneNumber = "+2222222222", RegistrationDate = DateTime.UtcNow },
                new Guest { FirstName = "Guest3", LastName = "Three", Email = "guest3@test.com", PhoneNumber = "+3333333333", RegistrationDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetAllGuests();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var guests = Assert.IsAssignableFrom<IEnumerable<GuestDto>>(okResult.Value);
            Assert.Equal(3, guests.Count());
        }

        [Fact]
        public async Task UpdateGuest_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var guest = new Guest
            {
                FirstName = "Original",
                LastName = "Name",
                Email = "original@test.com",
                PhoneNumber = "+1234567890",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);
            await context.SaveChangesAsync();

            var updateDto = new CreateGuestDto
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@test.com",
                PhoneNumber = "+0987654321",
                Address = "New Address"
            };

            // Act
            var result = await controller.UpdateGuest(guest.Id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedGuest = await context.Guests.FindAsync(guest.Id);
            Assert.Equal("Updated", updatedGuest.FirstName);
            Assert.Equal("updated@test.com", updatedGuest.Email);
        }

        [Fact]
        public async Task DeleteGuest_WithNoActiveBookings_ReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var guest = new Guest
            {
                FirstName = "Delete",
                LastName = "Me",
                Email = "delete@test.com",
                PhoneNumber = "+1234567890",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteGuest(guest.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var deletedGuest = await context.Guests.FindAsync(guest.Id);
            Assert.Null(deletedGuest);
        }

        [Fact]
        public async Task DeleteGuest_WithActiveBookings_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new GuestsController(context);

            var room = new Room
            {
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerNight = 50.00m,
                IsAvailable = true
            };
            context.Rooms.Add(room);

            var guest = new Guest
            {
                FirstName = "Active",
                LastName = "Guest",
                Email = "active@test.com",
                PhoneNumber = "+1234567890",
                RegistrationDate = DateTime.UtcNow
            };
            context.Guests.Add(guest);
            await context.SaveChangesAsync();

            var booking = new Booking
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(3),
                TotalPrice = 100.00m,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteGuest(guest.Id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
