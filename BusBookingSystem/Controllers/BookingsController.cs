using BusBookingSystem.Data;
using BusBookingSystem.Models;
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

    public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // POST: api/Bookings/select-seats
    [HttpPost("select-seats")]
    public async Task<IActionResult> SelectSeats(int busId, List<string> seatCodes)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var bus = await _context.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.Id == busId);
        if (bus == null) return NotFound("Bus not found");

        var seats = bus.Seats.Where(s => seatCodes.Contains(s.SeatCode)).ToList();
        if (seats.Count != seatCodes.Count) return BadRequest("Some seats not found");

        if (seats.Any(s => !s.IsAvailable || (s.LockedUntil.HasValue && s.LockedUntil > DateTime.Now)))
            return BadRequest("Some seats are not available");

        // Lock seats for 5 minutes
        var lockUntil = DateTime.Now.AddMinutes(5);
        foreach (var seat in seats)
        {
            seat.LockedUntil = lockUntil;
            seat.LockedByUserId = user.Id;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Seats locked for 5 minutes", LockUntil = lockUntil });
    }

    // POST: api/Bookings/confirm
    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmBooking(int busId, List<PassengerDetail> passengers, string paymentMethod)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var bus = await _context.Buses.Include(b => b.Seats).FirstOrDefaultAsync(b => b.Id == busId);
        if (bus == null) return NotFound("Bus not found");

        var seatCodes = passengers.Select(p => p.SeatNumber).ToList();
        var seats = bus.Seats.Where(s => seatCodes.Contains(s.SeatCode)).ToList();

        if (seats.Any(s => s.LockedByUserId != user.Id || s.LockedUntil < DateTime.Now))
            return BadRequest("Seats not locked or lock expired");

        // Simulate payment
        var paymentStatus = SimulatePayment(paymentMethod);
        if (paymentStatus != "Success")
        {
            // Release locks
            foreach (var seat in seats)
            {
                seat.LockedUntil = null;
                seat.LockedByUserId = null;
            }
            await _context.SaveChangesAsync();
            return BadRequest("Payment failed");
        }

        // Create booking
        var booking = new Booking
        {
            UserId = user.Id,
            BusId = busId,
            BookingDate = DateTime.Now,
            Status = "Booked",
            PassengerDetails = passengers
        };

        _context.Bookings.Add(booking);

        var payment = new Payment
        {
            Booking = booking,
            Amount = bus.Price * passengers.Count,
            Status = paymentStatus,
            PaymentDate = DateTime.Now
        };

        _context.Payments.Add(payment);

        // Mark seats as booked
        foreach (var seat in seats)
        {
            seat.IsAvailable = false;
            seat.LockedUntil = null;
            seat.LockedByUserId = null;
        }

        await _context.SaveChangesAsync();

        // Send email (placeholder)
        // await SendConfirmationEmail(user.Email, booking);

        return Ok(new { BookingId = booking.Id, Message = "Booking confirmed" });
    }

    private string SimulatePayment(string method)
    {
        // Dummy: 90% success, 10% failure
        return new Random().Next(10) < 9 ? "Success" : "Failure";
    }

    // GET: api/Bookings/my-bookings
    [HttpGet("my-bookings")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetMyBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var bookings = await _context.Bookings
            .Include(b => b.Bus)
            .Include(b => b.PassengerDetails)
            .Where(b => b.UserId == user.Id)
            .ToListAsync();

        return bookings;
    }
}