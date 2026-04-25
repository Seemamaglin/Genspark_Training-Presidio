using BusBookingSystem.Data;
using BusBookingSystem.Models;
using BusBookingSystem.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "BusOperator")]
public class BusOperatorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusOperatorsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetOperatorProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.Include(o => o.AssignedRoute)
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        return Ok(new
        {
            @operator.Id,
            user.Name,
            user.Email,
            @operator.Source,
            @operator.Destination,
            AssignedRoute = @operator.AssignedRoute == null ? null : new { @operator.AssignedRoute.Id, @operator.AssignedRoute.Source, @operator.AssignedRoute.Destination }
        });
    }

    [HttpGet("buses")]
    public async Task<IActionResult> GetMyBuses()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.Include(o => o.Buses).FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var buses = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.Seats)
            .Where(b => b.BusOperatorId == @operator.Id)
            .ToListAsync();

        return Ok(buses.Select(b => new
        {
            b.Id,
            b.RegistrationNumber,
            b.Timing,
            b.TravelDate,
            b.Price,
            b.IsActive,
            Route = new { b.Route.Source, b.Route.Destination },
            AvailableSeats = b.Seats.Count(s => s.IsAvailable && (s.LockedUntil == null || s.LockedUntil <= DateTime.UtcNow)),
            TotalSeats = b.Seats.Count
        }));
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetOperatorBookings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.Include(o => o.Buses).FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var bookings = await _context.Bookings
            .Include(b => b.Bus)
            .Include(b => b.User)
            .Include(b => b.Payment)
            .Include(b => b.PassengerDetails)
            .Where(b => @operator.Buses.Select(x => x.Id).Contains(b.BusId))
            .ToListAsync();

        return Ok(bookings.Select(b => new
        {
            b.Id,
            b.Status,
            b.BookingDate,
            Bus = new { b.Bus.RegistrationNumber, b.Bus.TravelDate, b.Bus.Timing, b.Bus.Price },
            User = new { b.User.Name, b.User.Email },
            Payment = b.Payment == null ? null : new { b.Payment.Status, b.Payment.Amount, b.Payment.PaymentMethod },
            Passengers = b.PassengerDetails.Select(p => new { p.Name, p.SeatNumber, p.Email, p.PhoneNumber, p.Age })
        }));
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetOperatorRevenue()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.Include(o => o.Buses).ThenInclude(b => b.Bookings).ThenInclude(b => b.Payment)
            .FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var revenue = @operator.Buses.SelectMany(b => b.Bookings)
            .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Success)
            .Sum(b => b.Payment?.Amount ?? 0);

        return Ok(new { Revenue = revenue });
    }

    [HttpPut("buses/{busId}/price")]
    public async Task<IActionResult> UpdateBusPrice(Guid busId, [FromBody] UpdateBusPriceRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.BusOperatorId == @operator.Id);
        if (bus == null) return NotFound("Bus not found.");

        bus.Price = request.Price;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Price updated." });
    }

    [HttpPut("buses/{busId}/disable")]
    public async Task<IActionResult> DisableBus(Guid busId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.BusOperatorId == @operator.Id);
        if (bus == null) return NotFound("Bus not found.");

        bus.IsActive = false;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Bus disabled." });
    }

    [HttpPut("buses/{busId}/enable")]
    public async Task<IActionResult> EnableBus(Guid busId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var @operator = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == user.Id);
        if (@operator == null) return NotFound("Operator profile not found.");

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == busId && b.BusOperatorId == @operator.Id);
        if (bus == null) return NotFound("Bus not found.");

        bus.IsActive = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Bus enabled." });
    }
}
