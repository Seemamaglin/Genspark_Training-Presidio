Build a Bus Booking Web Application similar to SETC / RedBus with the following tech stack and requirements.

Frontend: Angular

Backend: ASP.NET Core (.NET)

Database: PostgreSQL

Authentication: Role-based (User, Bus Operator, Admin)

Roles \& Permissions

&#x20;1. User Role

Can view available buses without login by selecting:

Source, Destination, Date

Bus list should show:

Available seats count

Seat layout with seat codes

Bus timing, route, price

Seat Selection:

User selects one or more seats

Selected seats are temporarily locked (reserved) for a few minutes

Other users cannot select these seats during this time

If payment succeeds → seats become booked

If payment fails / timeout → seats become available again

Login / Signup required only after seat selection

Booking \& Payment:

Create a dummy payment gateway (no external APIs)

On successful payment:

Store booking in DB

Send confirmation email to user

User Dashboard:

View travel history:

Upcoming journeys

Past journeys

Cancelled journeys

Passenger Details to store in DB:

Name

Phone number

Email

Age

Seat number

Source \& Destination

Payment details

Proof (ID reference or dummy field)

2\. Bus Operator Role

Default role is User

Must request role upgrade to Bus Operator

Admin must approve the request

Once approved, operator can:

Add buses only to routes assigned by Admin

While adding bus:

Bus registration number (mandatory)

Timing

Seat layout

Pricing (can vary by source/destination arc)

Update pricing

Temporarily disable/remove bus (e.g., breakdown)

View:

All bookings for their buses

Total revenue per journey / bus

Operator’s own source and destination define the route visible to users

Operator details (source, destination, bus info) must be stored in DB

3\. Admin Role

Full control of system

Can:

Create and manage routes

Define source and destination locations

Assign routes to bus operators

Approve or reject bus operator requests

Enable or disable operators

View and manage total revenue

Cancellation handling:

If a bus is cancelled:

Notify users via email with cancellation reason

Mention refund details

Notify bus operator via email

4\. Payment Gateway

Dummy payment flow only

No third-party payment APIs

Simulate:

Success

Failure

Timeout (seat release)

🧭 Route Logic (Important)

Routes are created by Admin

Users see source and destination based on bus operator’s registered route

Example:

If operator starts from their own address/location, that becomes the source

Route, operator, and bus mapping must be persisted in DB

🎨 UI / Layout

Can use:

Free templates from the internet OR

Simple custom layout

Reference design:

SETC

RedBus

🗄️ Database

PostgreSQL

Must include tables for:

Users \& Roles

Bus Operators

Buses

Routes

Seats

Bookings

Payments

Passenger details

Revenue

📩 Email

Send emails for:

Booking confirmation

Bus cancellation

Refund information

Operator approval / rejection

📌 Expectations

Clean architecture

Proper role-based authorization

Seat locking logic must be handled correctly

No requirement should be skipped

