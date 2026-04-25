using BusBookingSystem.Data;
using BusBookingSystem.DTOs.Requests;
using BusBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BusBookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public BusesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/Buses/search?source=...&destination=...&date=...
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> SearchBuses(string source, string destination, DateTime date)
    {
        var buses = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.BusOperator)
            .Include(b => b.Seats)
            .Where(b => b.Route.Source == source && b.Route.Destination == destination && b.IsActive && b.TravelDate.Date == date.Date)
            .ToListAsync();

        return buses.Select(b => new
        {
            b.Id,
            b.RegistrationNumber,
            Route = new { b.Route.Source, b.Route.Destination },
            b.Timing,
            b.TravelDate,
            b.Price,
            AvailableSeats = b.Seats.Count(s => s.IsAvailable && (s.LockedUntil == null || s.LockedUntil <= DateTime.UtcNow)),
            TotalSeats = b.Seats.Count,
            SeatCodes = b.Seats.Select(s => s.SeatCode),
            b.IsActive
        }).ToList();
    }

    // POST: api/Buses
    [HttpPost]
    [Authorize(Policy = "BusOperator")]
    public async Task<ActionResult<Bus>> CreateBus(CreateBusRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var busOperator = await _context.BusOperators.Include(o => o.AssignedRoute).FirstOrDefaultAsync(o => o.UserId == currentUser.Id && o.IsEnabled);
        if (busOperator == null)
        {
            return BadRequest("Bus operator profile not found or not enabled.");
        }

        if (busOperator.AssignedRouteId == null)
        {
            return BadRequest("Bus operator does not have an assigned route.");
        }

        var route = await _context.Routes.FindAsync(busOperator.AssignedRouteId.Value);
        if (route == null)
        {
            return NotFound("Assigned route not found.");
        }

        var bus = new Bus
        {
            RegistrationNumber = request.RegistrationNumber,
            BusOperatorId = busOperator.Id,
            RouteId = route.Id,
            Timing = request.Timing,
            TravelDate = request.TravelDate.ToUniversalTime(),
            SeatLayout = request.SeatLayout,
            Price = request.Price,
            IsActive = true
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        var seatCodes = ParseSeatCodes(request.SeatLayout);
        foreach (var seatCode in seatCodes)
        {
            _context.Seats.Add(new Seat
            {
                BusId = bus.Id,
                SeatCode = seatCode,
                IsAvailable = true
            });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBus), new { id = bus.Id }, bus);
    }

    private static IEnumerable<string> ParseSeatCodes(string seatLayout)
    {
        if (string.IsNullOrWhiteSpace(seatLayout)) return Array.Empty<string>();

        try
        {
            var parsed = JsonSerializer.Deserialize<List<string>>(seatLayout);
            if (parsed != null && parsed.Any()) return parsed.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
        }
        catch
        {
            // ignore invalid JSON and fallback to comma-separated parsing
        }

        return seatLayout.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    // GET: api/Buses/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Bus>> GetBus(Guid id)
    {
        var bus = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.Seats)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null)
        {
            return NotFound();
        }

        return bus;
    }

    // PUT: api/Buses/5
    [HttpPut("{id}")]
    [Authorize(Policy = "BusOperator")]
    public async Task<IActionResult> UpdateBus(Guid id, Bus bus)
    {
        if (id != bus.Id)
        {
            return BadRequest();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var operatorProfile = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == currentUser.Id && o.IsEnabled);
        if (operatorProfile == null) return BadRequest("Bus operator profile not found.");

        var existingBus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.BusOperatorId == operatorProfile.Id);
        if (existingBus == null) return NotFound("Bus not found or not owned by this operator.");

        existingBus.RegistrationNumber = bus.RegistrationNumber;
        existingBus.Timing = bus.Timing;
        existingBus.TravelDate = bus.TravelDate;
        existingBus.Price = bus.Price;
        existingBus.IsActive = bus.IsActive;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BusExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Buses/5
    [HttpDelete("{id}")]
    [Authorize(Policy = "BusOperator")]
    public async Task<IActionResult> DeleteBus(Guid id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var operatorProfile = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == currentUser.Id && o.IsEnabled);
        if (operatorProfile == null) return BadRequest("Bus operator profile not found.");

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.BusOperatorId == operatorProfile.Id);
        if (bus == null)
        {
            return NotFound();
        }

        bus.IsActive = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BusExists(Guid id)
    {
        return _context.Buses.Any(e => e.Id == id);
    }
}