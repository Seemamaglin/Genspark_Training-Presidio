# Bus Booking System — Implementation Guide

> A full-stack bus ticket booking application inspired by RedBus / SETC.
> Built with Angular 16 (frontend), ASP.NET Core 8 (backend), and PostgreSQL (database).

---

## Table of Contents
1. [Project Overview](#1-project-overview)
2. [Architecture](#2-architecture)
3. [Tech Stack](#3-tech-stack)
4. [Database Schema](#4-database-schema)
5. [User Roles & Permissions](#5-user-roles--permissions)
6. [Key Features & How They Work](#6-key-features--how-they-work)
7. [API Reference](#7-api-reference)
8. [Local Setup Guide](#8-local-setup-guide)
9. [Default Credentials](#9-default-credentials)
10. [Project File Structure](#10-project-file-structure)

---

## 1. Project Overview

This is a complete bus ticket booking system where:
- **Passengers** can search buses, pick seats, and pay online
- **Bus Operators** can add and manage their buses on assigned routes
- **Admins** control routes, approve operators, and monitor revenue

The payment is simulated (no real payment gateway needed). Emails are sent via SMTP — if SMTP is not configured, emails are logged to the console instead.

---

## 2. Architecture

```
Browser (Angular 16)
        │
        │  HTTP / REST API (JSON)
        ▼
ASP.NET Core 8 Web API
        │
        ├── Controllers (5)
        ├── Services (EmailService, JwtTokenService)
        ├── Entity Framework Core (ORM)
        │
        ▼
PostgreSQL Database
```

**How a request flows:**
1. User clicks something in the Angular app
2. Angular's `ApiService` makes an HTTP call with the JWT token in the `Authorization` header
3. The ASP.NET Core API validates the JWT, checks the role, and processes the request
4. Data is read/written to PostgreSQL via Entity Framework Core
5. The API returns JSON, Angular updates the view

---

## 3. Tech Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | Angular | 16.2 |
| Backend | ASP.NET Core | 8.0 |
| Database | PostgreSQL | 14+ |
| ORM | Entity Framework Core + Npgsql | 8.0 |
| Auth | ASP.NET Identity + JWT Bearer | — |
| Email | MailKit (SMTP) | — |
| CSS | Custom (RedBus-inspired) | — |

---

## 4. Database Schema

These tables are created automatically when you run `dotnet ef database update`:

| Table | What it stores |
|-------|----------------|
| `AspNetUsers` | All users (passengers, operators, admins) |
| `AspNetRoles` | Roles: User, BusOperator, Admin |
| `BusOperators` | Approved bus operators and their assigned routes |
| `Routes` | Source → Destination pairs created by admin |
| `Buses` | Bus details: registration, timing, date, price, seat layout |
| `Seats` | Individual seat rows — one row per seat per bus |
| `Bookings` | A confirmed booking made by a user |
| `PassengerDetails` | Name, phone, age for each passenger in a booking |
| `Payments` | Payment record for each booking (simulated) |
| `Revenues` | Running revenue totals per operator |

**Key relationship:**
```
Route → Bus → Seat
               ↑
User ──── Booking → PassengerDetail
               ↓
           Payment
```

---

## 5. User Roles & Permissions

### User (default role after signup)
- Search buses without logging in
- Select seats (seats are temporarily locked for 5 minutes)
- Login/signup only required when confirming payment
- View upcoming, past, and cancelled bookings
- Cancel a booking and get a refund
- Request upgrade to Bus Operator role

### Bus Operator (promoted by Admin)
- Add buses to their assigned route
- Set bus timing, date, seat layout, and price
- Enable/disable buses (e.g., for breakdown)
- View all bookings made on their buses
- View total revenue earned

### Admin (seeded automatically)
- Create and manage routes (source/destination pairs)
- Review and approve/reject bus operator upgrade requests
- Assign a route to an approved operator
- Disable/enable operators
- Cancel any bus (auto-refunds all bookings and emails passengers)
- View all users, buses, and total revenue

---

## 6. Key Features & How They Work

### Seat Locking (5-minute reservation)
When a user clicks "Reserve Seats":
1. Backend checks the seats are available and not locked
2. Each selected seat gets `LockedUntil = now + 5 minutes` and a `ReservationToken` (GUID)
3. No other user can select these seats during this window
4. If the user completes payment → seats become `IsAvailable = false` (permanently booked)
5. If payment times out or fails → seats are released back to available
6. Expired locks are cleaned up automatically on the next seat selection request for that bus

### Dummy Payment Gateway
Three outcomes are simulated:
- **Card / UPI**: 80% chance of success, 15% chance of failure, 5% chance of timeout
- **Timeout (manual)**: Select "Simulate Timeout" to force a timeout — seats are released immediately

### Email Notifications
Emails are sent (or logged if SMTP is not configured) for:
- Booking confirmation (to passenger)
- Bus cancellation with refund details (to all affected passengers)
- Operator approval or rejection (to the applicant)
- Bus cancellation notice (to the bus operator)

### Operator Approval Flow
```
User clicks "Request Operator Upgrade"
        ↓
Admin sees request in "Pending Requests" panel
        ↓
Admin selects a route and clicks Approve
        ↓
User is added to "BusOperator" role in the database
        ↓
Operator can now add buses to their assigned route
```

---

## 7. API Reference

All endpoints are prefixed with `/api`. Authenticated endpoints require `Authorization: Bearer <token>` header.

### Account
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| POST | `/api/Account/register` | Public | Create new user account |
| POST | `/api/Account/login` | Public | Login, returns JWT token |
| POST | `/api/Account/request-operator-upgrade` | User | Request bus operator role |
| GET | `/api/Account/me` | Any | Get own profile |

### Buses
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| GET | `/api/Buses/search?source=&destination=&date=` | Public | Search available buses |
| GET | `/api/Buses/{id}` | Public | Get bus details + seat layout |
| POST | `/api/Buses` | BusOperator | Create a new bus |
| PUT | `/api/Buses/{id}` | BusOperator | Update bus details |

### Bookings
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| POST | `/api/Bookings/select-seats` | Public | Lock seats for 5 minutes |
| POST | `/api/Bookings/confirm` | User | Confirm booking + run payment |
| GET | `/api/Bookings/my-bookings` | User | All bookings for logged-in user |
| GET | `/api/Bookings/dashboard` | User | Upcoming / past / cancelled split |
| POST | `/api/Bookings/cancel/{bookingId}` | User | Cancel booking, trigger refund |

### Bus Operators
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| GET | `/api/BusOperators/me` | BusOperator | Own operator profile |
| GET | `/api/BusOperators/buses` | BusOperator | All buses owned by operator |
| GET | `/api/BusOperators/bookings` | BusOperator | All bookings on operator's buses |
| GET | `/api/BusOperators/revenue` | BusOperator | Total revenue earned |
| PUT | `/api/BusOperators/buses/{id}/disable` | BusOperator | Disable a bus |
| PUT | `/api/BusOperators/buses/{id}/enable` | BusOperator | Re-enable a bus |
| PUT | `/api/BusOperators/buses/{id}/price` | BusOperator | Update seat price |

### Admin
| Method | URL | Auth | Description |
|--------|-----|------|-------------|
| GET/POST | `/api/Admin/routes` | Admin | List / create routes |
| PUT/DELETE | `/api/Admin/routes/{id}` | Admin | Update / delete route |
| GET | `/api/Admin/operator-requests` | Admin | Pending upgrade requests |
| POST | `/api/Admin/operators/{id}/approve` | Admin | Approve + assign route |
| POST | `/api/Admin/operators/{id}/reject` | Admin | Reject with reason |
| POST | `/api/Admin/operators/{id}/disable` | Admin | Disable operator |
| POST | `/api/Admin/operators/{id}/enable` | Admin | Re-enable operator |
| GET | `/api/Admin/operators` | Admin | List all approved operators |
| GET | `/api/Admin/buses` | Admin | List all buses |
| POST | `/api/Admin/buses/{id}/cancel` | Admin | Cancel bus, refund all bookings |
| GET | `/api/Admin/revenue` | Admin | Total + per-operator revenue |
| GET | `/api/Admin/users` | Admin | All registered users |

---

## 8. Local Setup Guide

### Prerequisites
Install these before starting:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) — to run the backend
- [Node.js 18+](https://nodejs.org) — to run the frontend
- [PostgreSQL 14+](https://www.postgresql.org/download/) — the database

### Step 1: Configure the Database

Open `appsettings.json` in `BusBookingSystem/` and update the connection string:
```json
"DefaultConnection": "Host=localhost;Port=5432;Database=bus_booking_db;Username=postgres;Password=YOUR_PASSWORD"
```
Replace `YOUR_PASSWORD` with your PostgreSQL password.

Create the database in PostgreSQL (you can use pgAdmin or the command line):
```sql
CREATE DATABASE bus_booking_db;
```

### Step 2: Run Database Migrations

Open a terminal in the `BusBookingSystem/` folder and run:
```bash
dotnet ef database update
```
This creates all the tables and seeds the default admin user automatically.

### Step 3: Start the Backend

In the `BusBookingSystem/` folder:
```bash
dotnet run
```
The API will start at `https://localhost:5001`. You can browse the API docs at `https://localhost:5001/swagger`.

### Step 4: Start the Frontend

Open a second terminal in `BusBookingSystem/ClientApp/`:
```bash
npm install --legacy-peer-deps
npm start
```
Angular will open the app in your browser at `http://localhost:4200`.

### Step 5: Configure Email (Optional)

To send real emails, update `appsettings.json`:
```json
"Smtp": {
  "Host": "smtp.gmail.com",
  "Port": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "From": "your-email@gmail.com"
}
```
If SMTP is left as placeholder values, emails are **logged to the console** instead — the app works normally without it.

---

## 9. Default Credentials

After running migrations, a default admin account is created:

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@busbooking.com` | `Admin@1234` |

**To test the full flow:**
1. Register as a **User** → search buses → book a ticket
2. Login as **Admin** → create a route → approve an operator request
3. Register a second account → request Bus Operator upgrade → login as Admin → approve it
4. Login as **Bus Operator** → add a bus to the route

---

## 10. Project File Structure

```
BusBookingSystem/
│
├── Controllers/
│   ├── AccountController.cs     # Register, Login, Operator upgrade
│   ├── BusesController.cs       # Bus CRUD + search
│   ├── BookingsController.cs    # Seat locking, booking, cancellation
│   ├── BusOperatorsController.cs# Operator dashboard endpoints
│   └── AdminController.cs       # Admin: routes, operators, revenue
│
├── Models/
│   ├── ApplicationUser.cs       # User identity (extends IdentityUser)
│   ├── Bus.cs                   # Bus entity
│   ├── Route.cs                 # Route entity (source → destination)
│   ├── Seat.cs                  # Seat with lock fields
│   ├── Booking.cs               # Booking with status
│   ├── Payment.cs               # Simulated payment record
│   ├── PassengerDetail.cs       # Per-seat passenger info
│   └── BusOperator.cs           # Operator profile
│
├── Services/
│   ├── JwtTokenService.cs       # Creates JWT tokens (static utility)
│   └── EmailService.cs          # Sends emails via SMTP / logs to console
│
├── Data/
│   └── ApplicationDbContext.cs  # EF Core database context
│
├── DTOs/
│   ├── Requests/                # Input shapes (register, login, booking, etc.)
│   └── Responses/               # Output shapes (auth response, booking, etc.)
│
├── Migrations/                  # Auto-generated EF Core migration files
├── Program.cs                   # App startup, DI, middleware, role seeding
└── appsettings.json             # Config: DB, JWT, SMTP
│
ClientApp/                       # Angular 16 frontend
├── src/app/
│   ├── pages/
│   │   ├── home/                # Landing page
│   │   ├── auth/                # Login & Register
│   │   ├── search/              # Bus search results
│   │   ├── booking/             # Seat selection + payment
│   │   ├── dashboard/           # User's booking history
│   │   ├── operator/            # Bus operator panel
│   │   └── admin/               # Admin control panel
│   ├── services/
│   │   ├── api.service.ts       # All HTTP calls to the backend
│   │   └── auth.service.ts      # JWT token + role management
│   └── app.component.*          # Root component with navigation
└── src/styles.css               # Global styles
```
