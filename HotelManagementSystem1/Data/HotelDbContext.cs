using HotelManagementSystem1.Models;
using HotelManagementSystem1.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem1.Data
{
    public class HotelDbContext : DbContext
    {
        public HotelDbContext(DbContextOptions<HotelDbContext> options)
            : base(options)
        {
        }

        public DbSet<Guest> Guests { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Guest entity
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.HasKey(g => g.Id);
                entity.HasIndex(g => g.Email).IsUnique();
                entity.Property(g => g.Email).IsRequired();
                entity.Property(g => g.FirstName).HasMaxLength(50);
                entity.Property(g => g.LastName).HasMaxLength(50);
            });

            // Configure Room entity
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.HasIndex(r => r.RoomNumber).IsUnique();
                entity.Property(r => r.RoomNumber).IsRequired().HasMaxLength(10);
                entity.Property(r => r.PricePerNight).HasColumnType("decimal(18,2)");
            });

            // Configure Booking entity and relationships
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.TotalPrice).HasColumnType("decimal(18,2)");
                entity.Property(b => b.Status).HasMaxLength(20);

                // One-to-Many: Guest -> Bookings
                entity.HasOne(b => b.Guest)
                    .WithMany(g => g.Bookings)
                    .HasForeignKey(b => b.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One-to-Many: Room -> Bookings
                entity.HasOne(b => b.Room)
                    .WithMany(r => r.Bookings)
                    .HasForeignKey(b => b.RoomId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Review entity
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(r => r.Id);

                // One-to-Many: Guest -> Reviews
                entity.HasOne(r => r.Guest)
                    .WithMany()
                    .HasForeignKey(r => r.GuestId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Rooms
            modelBuilder.Entity<Room>().HasData(
                new Room
                {
                    Id = 1,
                    RoomNumber = "101",
                    RoomType = "Single",
                    Capacity = 1,
                    PricePerNight = 50.00m,
                    IsAvailable = true,
                    Description = "Cozy single room with city view"
                },
                new Room
                {
                    Id = 2,
                    RoomNumber = "102",
                    RoomType = "Double",
                    Capacity = 2,
                    PricePerNight = 80.00m,
                    IsAvailable = true,
                    Description = "Comfortable double room with queen bed"
                },
                new Room
                {
                    Id = 3,
                    RoomNumber = "201",
                    RoomType = "Suite",
                    Capacity = 4,
                    PricePerNight = 150.00m,
                    IsAvailable = true,
                    Description = "Luxury suite with living area and balcony"
                }
            );

            // Seed Admin and Receptionist users with STATIC dates
            modelBuilder.Entity<Guest>().HasData(
                new Guest
                {
                    Id = 1,
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "admin@hotel.com",
                    PhoneNumber = "+2764567890",
                    Address = "Hotel Office",
                    PasswordHash = PasswordService.HashPassword("admin123"),
                    Role = "Admin",
                    RegistrationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) // STATIC DATE
                },
                new Guest
                {
                    Id = 2,
                    FirstName = "Reception",
                    LastName = "Desk",
                    Email = "reception@hotel.com",
                    PhoneNumber = "+2787654321",
                    Address = "Hotel Front Desk",
                    PasswordHash = PasswordService.HashPassword("reception123"),
                    Role = "Receptionist",
                    RegistrationDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) // STATIC DATE
                }
            );
        }
    }
}
