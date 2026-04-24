# Bus Booking System

A comprehensive bus booking web application built with ASP.NET Core backend and PostgreSQL database.

## Features

- Role-based authentication (User, Bus Operator, Admin)
- Bus search and booking
- Seat selection with temporary locking
- Dummy payment gateway
- Email notifications
- Admin panel for route and operator management

## Tech Stack

- Backend: ASP.NET Core (.NET 8)
- Database: PostgreSQL
- Authentication: JWT with Identity
- ORM: Entity Framework Core

## Setup Instructions

### Prerequisites

1. Install .NET 8 SDK: https://dotnet.microsoft.com/download
2. Install PostgreSQL:
   - Download from: https://www.postgresql.org/download/windows/
   - Install and create a database named `BusBookingDb`
   - Note the username and password for the database
3. Install Entity Framework tools: `dotnet tool install --global dotnet-ef`

### Database Setup

1. Update `appsettings.json` with your PostgreSQL connection string:
   ```
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=BusBookingDb;Username=yourusername;Password=yourpassword"
   }
   ```

2. Update JWT settings in `appsettings.json`:
   ```
   "Jwt": {
     "Key": "YourSuperSecretKeyHere",
     "Issuer": "BusBookingSystem",
     "Audience": "BusBookingSystem"
   }
   ```

### Running the Application

1. Restore packages:
   ```
   dotnet restore
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Create and run migrations:
   ```
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. Run the application:
   ```
   dotnet run
   ```

The API will be available at `https://localhost:5001` with Swagger UI.

## API Endpoints

- `GET /api/Buses/search?source=...&destination=...&date=...` - Search buses
- `POST /api/Bookings/select-seats` - Lock seats
- `POST /api/Bookings/confirm` - Confirm booking with payment
- `GET /api/Bookings/my-bookings` - Get user bookings

## Database Schema

Tables: Users, Roles, BusOperators, Buses, Routes, Seats, Bookings, Payments, PassengerDetails, Revenues

## Notes

- Seat locking is implemented with a 5-minute timeout
- Payment is simulated (90% success rate)
- Email sending is placeholder (needs SMTP configuration)
- Admin approval for bus operators is manual in DB for now

## Next Steps

- Implement Angular frontend
- Add email service with SMTP
- Implement background job for lock release
- Add more admin controllers
- Add validation and error handling