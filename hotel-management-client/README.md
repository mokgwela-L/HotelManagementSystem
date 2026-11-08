# Hotel Management System

A full-stack web application for managing hotel operations including guest registration, room management, and booking reservations.

## ?? Features

- **Guest Management**: Register, update, and manage guest information
- **Room Management**: Add, update, and track room availability
- **Booking System**: Create and manage reservations with automatic price calculation
- **Dashboard**: View and manage all bookings 
- **Duplicate Prevention**: Automatic checking for duplicate emails and room numbers
- **Overlapping Prevention**: Prevents double-booking of rooms
- **Status Management**: Track booking status (Pending, Confirmed, Completed, Cancelled)

## ??? Technologies Used

### Back-End
- **ASP.NET Core 6.0+** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **Swagger/OpenAPI** - API Documentation

### Front-End
- **React 18** - UI Framework
- **React Router** - Navigation
- **Axios** - HTTP Client
- **CSS3** - Styling

### Testing
- **xUnit** - Unit Testing Framework
- **InMemory Database** - Integration Testing

## ?? Authentication System

### Features
- User registration with password
- Password hashing (SHA256)
- Login/Logout functionality
- Session management with localStorage
- Protected routes
- Dynamic navigation based on authentication state

### Usage
1. **Register**: Create an account with email and password
2. **Login**: Use your credentials to access the system
3. **Logout**: Click logout button to end your session

### Security
- Passwords are hashed before storage
- Minimum password length: 6 characters
- Password confirmation required during registration
- Session stored in browser localStorage

## ?? Prerequisites

- Visual Studio 2022 or later
- .NET 6.0 SDK or later
- SQL Server (LocalDB, Express, or Full)
- Node.js 16+ and npm
- Visual Studio Code (for React development)

## ?? Setup Instructions

### 1. Clone/Download the Project
```bash
git clone <your-repository-url>
cd HotelManagementSystem
