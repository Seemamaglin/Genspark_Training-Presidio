using BusBookingSystem.Data;
using BusBookingSystem.DTOs.Requests;
using BusBookingSystem.DTOs.Responses;
using BusBookingSystem.Models;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, EmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    // POST: api/Bookings/select-seats
    [HttpPost("select-seats")]
    [AllowAnonymous]
    public async Task<IActionResult> SelectSeats([FromBody] SelectSeatsRequest request)
    {
        await ReleaseExpiredSeatLocksAsync(request.BusId);

        var bus = await _context.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.Id == request.BusId);
        if (bus == null) return NotFound("Bus not found");

        var seats = bus.Seats.Where(s => request.SeatCodes.Contains(s.SeatCode)).ToList();
        if (seats.Count != request.SeatCodes.Count) return BadRequest("Some seats not found.");

        if (seats.Any(s => !s.IsAvailable || (s.LockedUntil.HasValue && s.LockedUntil > DateTime.UtcNow)))
        {
            return BadRequest("Some seats are not available.");
        }

        var reservationToken = request.ReservationToken;
        if (string.IsNullOrWhiteSpace(reservationToken))
        {
            reservationToken = Guid.NewGuid().ToString();
        }

        var lockUntil = DateTime.UtcNow.AddMinutes(5);
        foreach (var seat in seats)
        {
            seat.LockedUntil = lockUntil;
            seat.LockedBySessionId = reservationToken;
            seat.LockedByUserId = null;
        }

        await _context.SaveChangesAsync();

        return Ok(new SeatReservationResponse
        {
            ReservationToken = reservationToken,
            LockedUntil = lockUntil
        });
    }

    // POST: api/Bookings/confirm
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmBooking([FromBody] ConfirmBookingRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var bus = await _context.Buses
            .Include(b => b.Seats)
            .Include(b => b.Route)
            .FirstOrDefaultAsync(b => b.Id == request.BusId);
        if (bus == null) return NotFound("Bus not found");

        await ReleaseExpiredSeatLocksAsync(request.BusId);

        var seatCodes = request.PassengerDetails.Select(p => p.SeatNumber).ToList();
        var seats = bus.Seats.Where(s => seatCodes.Contains(s.SeatCode)).ToList();
        if (seats.Count != seatCodes.Count)
        {
            return BadRequest("Some passenger seat codes are invalid.");
        }

        if (seats.Any(s => s.LockedUntil == null || s.LockedUntil < DateTime.UtcNow || s.LockedBySessionId != request.ReservationToken))
        {
            return BadRequest("Seats are not locked or the reservation token is invalid.");
        }

        var paymentOutcome = SimulatePayment(request.PaymentMethod);
        var paymentStatus = paymentOutcome == "Success" ? PaymentStatus.Success : PaymentStatus.Failed;

        if (paymentOutcome == "Timeout")
        {
            ReleaseLockedSeats(seats);
            await _context.SaveChangesAsync();
            return StatusCode(408, "Payment timeout. Seats are released.");
        }

        if (paymentStatus != PaymentStatus.Success)
        {
            ReleaseLockedSeats(seats);
            await _context.SaveChangesAsync();
            return BadRequest("Payment failed.");
        }

        var amount = bus.Price * request.PassengerDetails.Count;
        var bookingReference = $"BB-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..6]}";

        var booking = new Booking
        {
            UserId = user.Id,
            BusId = request.BusId,
            BookingDate = DateTime.UtcNow,
            Status = BookingStatus.Confirmed,
            TotalAmount = amount,
            BookingReference = bookingReference,
            PassengerDetails = request.PassengerDetails
        };

        _context.Bookings.Add(booking);

        var payment = new Payment
        {
            Booking = booking,
            Amount = amount,
            Status = paymentStatus,
            PaymentMethod = request.PaymentMethod,
            TransactionId = Guid.NewGuid().ToString(),
            SimulatedOutcome = paymentOutcome,
            PaymentDate = DateTime.UtcNow,
            InitiatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);

        foreach (var seat in seats)
        {
            seat.IsAvailable = false;
            seat.LockedUntil = null;
            seat.LockedBySessionId = null;
            seat.LockedByUserId = user.Id;
        }

        await _context.SaveChangesAsync();

        await _emailService.SendBookingConfirmationEmailAsync(
            user.Email ?? string.Empty,
            user.Name,
            bus.RegistrationNumber,
            bus.Route.Source,
            bus.Route.Destination,
            string.Join(", ", seatCodes),
            amount);

        return Ok(new { BookingId = booking.Id, Message = "Booking confirmed.", Reference = bookingReference });
    }

    [HttpGet("my-bookings")]
    public async Task<ActionResult<IEnumerable<BookingResponseDto>>> GetMyBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var bookings = await _context.Bookings
            .Include(b => b.Bus)
            .ThenInclude(bu => bu.Route)
            .Include(b => b.PassengerDetails)
            .Include(b => b.Payment)
            .Where(b => b.UserId == user.Id)
            .ToListAsync();

        var response = bookings.Select(b => new BookingResponseDto
        {
            Id = b.Id,
            BookingReference = b.BookingReference,
            Status = b.Status.ToString(),
            BookingDate = b.BookingDate,
            Bus = new BusResponseDto
            {
                RegistrationNumber = b.Bus.RegistrationNumber,
                TravelDate = b.Bus.TravelDate,
                Timing = b.Bus.Timing,
                Route = new RouteResponseDto
                {
                    Source = b.Bus.Route.Source,
                    Destination = b.Bus.Route.Destination
                }
            },
            TotalAmount = b.TotalAmount,
            Payment = b.Payment == null ? null : new PaymentResponseDto
            {
                Status = b.Payment.Status.ToString(),
                Amount = b.Payment.Amount,
                PaymentMethod = b.Payment.PaymentMethod,
                TransactionId = b.Payment.TransactionId
            },
            Passengers = b.PassengerDetails.Select(p => new PassengerResponseDto
            {
                Name = p.Name,
                SeatNumber = p.SeatNumber,
                Email = p.Email,
                PhoneNumber = p.PhoneNumber,
                Age = p.Age
            }).ToList()
        });

        return Ok(response);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetBookingDashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var today = DateTime.UtcNow.Date;
        var bookings = await _context.Bookings
            .Include(b => b.Bus)
            .ThenInclude(bu => bu.Route)
            .Include(b => b.Payment)
            .Where(b => b.UserId == user.Id)
            .ToListAsync();

        var upcoming = bookings.Where(b => b.Status == BookingStatus.Confirmed && b.Bus.TravelDate.Date >= today).ToList();
        var past = bookings.Where(b => b.Status == BookingStatus.Confirmed && b.Bus.TravelDate.Date < today).ToList();
        var cancelled = bookings.Where(b => b.Status == BookingStatus.Cancelled || (b.Payment != null && b.Payment.Status == PaymentStatus.Refunded)).ToList();

        return Ok(new
        {
            Upcoming = upcoming.Select(b => CreateBookingSummary(b)),
            Past = past.Select(b => CreateBookingSummary(b)),
            Cancelled = cancelled.Select(b => CreateBookingSummary(b))
        });
    }

    [HttpPost("cancel/{bookingId}")]
    public async Task<IActionResult> CancelBooking(Guid bookingId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Bus)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id);
        if (booking == null) return NotFound("Booking not found.");

        if (booking.Status == BookingStatus.Cancelled) return BadRequest("Booking already cancelled.");

        booking.Status = BookingStatus.Cancelled;
        if (booking.Payment != null)
        {
            booking.Payment.Status = PaymentStatus.Refunded;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Booking cancelled and refund processed." });
    }

    private object CreateBookingSummary(Booking booking)
    {
        return new
        {
            booking.Id,
            booking.BookingReference,
            booking.Status,
            booking.BookingDate,
            booking.TotalAmount,
            booking.Bus.TravelDate,
            booking.Bus.Timing,
            Route = new { booking.Bus.Route.Source, booking.Bus.Route.Destination },
            Payment = booking.Payment == null ? null : new { booking.Payment.Status, booking.Payment.Amount },
        };
    }

    private async Task ReleaseExpiredSeatLocksAsync(Guid busId)
    {
        var seats = await _context.Seats.Where(s => s.BusId == busId && s.LockedUntil != null && s.LockedUntil < DateTime.UtcNow).ToListAsync();
        if (!seats.Any()) return;

        foreach (var seat in seats)
        {
            seat.LockedUntil = null;
            seat.LockedBySessionId = null;
            seat.LockedByUserId = null;
        }

        await _context.SaveChangesAsync();
    }

    private void ReleaseLockedSeats(IEnumerable<Seat> seats)
    {
        foreach (var seat in seats)
        {
            seat.LockedUntil = null;
            seat.LockedBySessionId = null;
            seat.LockedByUserId = null;
        }
    }

    private string SimulatePayment(string method)
    {
        if (string.Equals(method, "timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "Timeout";
        }

        var roll = new Random().Next(100);
        if (roll < 80) return "Success";
        if (roll < 95) return "Failure";
        return "Timeout";
    }
}
