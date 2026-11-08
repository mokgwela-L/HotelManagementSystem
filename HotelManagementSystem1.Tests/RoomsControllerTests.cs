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
    public class RoomsControllerTests
    {
        private HotelDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<HotelDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new HotelDbContext(options);
        }

        [Fact]
        public async Task CreateRoom_WithValidData_ReturnsCreatedResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            var newRoom = new CreateRoomDto
            {
                RoomNumber = "301",
                RoomType = "Deluxe",
                Capacity = 3,
                PricePerNight = 120.00m,
                Description = "Luxury deluxe room"
            };

            // Act
            var result = await controller.CreateRoom(newRoom);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnedRoom = Assert.IsType<RoomDto>(createdResult.Value);
            Assert.Equal("301", returnedRoom.RoomNumber);
            Assert.Equal("Deluxe", returnedRoom.RoomType);
            Assert.Equal(120.00m, returnedRoom.PricePerNight);
            Assert.True(returnedRoom.IsAvailable);
        }

        [Fact]
        public async Task CreateRoom_WithDuplicateRoomNumber_ReturnsConflict()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            var existingRoom = new Room
            {
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerNight = 50.00m,
                IsAvailable = true
            };
            context.Rooms.Add(existingRoom);
            await context.SaveChangesAsync();

            var newRoom = new CreateRoomDto
            {
                RoomNumber = "101",
                RoomType = "Double",
                Capacity = 2,
                PricePerNight = 80.00m
            };

            // Act
            var result = await controller.CreateRoom(newRoom);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetAvailableRooms_ReturnsOnlyAvailableRooms()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            context.Rooms.AddRange(
                new Room { RoomNumber = "101", RoomType = "Single", Capacity = 1, PricePerNight = 50m, IsAvailable = true },
                new Room { RoomNumber = "102", RoomType = "Double", Capacity = 2, PricePerNight = 80m, IsAvailable = false },
                new Room { RoomNumber = "103", RoomType = "Suite", Capacity = 4, PricePerNight = 150m, IsAvailable = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetAvailableRooms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var rooms = Assert.IsAssignableFrom<IEnumerable<RoomDto>>(okResult.Value);
            Assert.Equal(2, rooms.Count());
            Assert.All(rooms, room => Assert.True(room.IsAvailable));
        }

        [Fact]
        public async Task GetRoomsByType_WithValidType_ReturnsMatchingRooms()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            context.Rooms.AddRange(
                new Room { RoomNumber = "101", RoomType = "Single", Capacity = 1, PricePerNight = 50m, IsAvailable = true },
                new Room { RoomNumber = "102", RoomType = "Double", Capacity = 2, PricePerNight = 80m, IsAvailable = true },
                new Room { RoomNumber = "103", RoomType = "Single", Capacity = 1, PricePerNight = 55m, IsAvailable = true }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetRoomsByType("Single");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var rooms = Assert.IsAssignableFrom<IEnumerable<RoomDto>>(okResult.Value);
            Assert.Equal(2, rooms.Count());
            Assert.All(rooms, room => Assert.Equal("Single", room.RoomType));
        }

        [Fact]
        public async Task GetRoomsByType_WithInvalidType_ReturnsNotFound()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            context.Rooms.Add(new Room
            {
                RoomNumber = "101",
                RoomType = "Single",
                Capacity = 1,
                PricePerNight = 50m,
                IsAvailable = true
            });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetRoomsByType("Presidential");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task UpdateRoom_WithValidData_ReturnsNoContent()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            var room = new Room
            {
                RoomNumber = "201",
                RoomType = "Double",
                Capacity = 2,
                PricePerNight = 80m,
                IsAvailable = true
            };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            var updateDto = new CreateRoomDto
            {
                RoomNumber = "201",
                RoomType = "Deluxe",
                Capacity = 3,
                PricePerNight = 120m,
                Description = "Updated description"
            };

            // Act
            var result = await controller.UpdateRoom(room.Id, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updatedRoom = await context.Rooms.FindAsync(room.Id);
            Assert.Equal("Deluxe", updatedRoom.RoomType);
            Assert.Equal(120m, updatedRoom.PricePerNight);
        }

        [Fact]
        public async Task DeleteRoom_WithActiveBookings_ReturnsBadRequest()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new RoomsController(context);

            var guest = new Guest
            {
                FirstName = "Test",
                LastName = "Guest",
                Email = "test@example.com",
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

            var booking = new Booking
            {
                GuestId = guest.Id,
                RoomId = room.Id,
                CheckInDate = DateTime.UtcNow.AddDays(1),
                CheckOutDate = DateTime.UtcNow.AddDays(3),
                TotalPrice = 100m,
                Status = "Confirmed",
                BookingDate = DateTime.UtcNow
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteRoom(room.Id);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
