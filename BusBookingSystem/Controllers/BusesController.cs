using BusBookingSystem.Data;
using BusBookingSystem.DTOs.Requests;
using BusBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> SearchBuses(string source, string destination, DateTime date)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var utcNext = utcDate.AddDays(1);
        var buses = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.BusOperator).ThenInclude(op => op!.User)
            .Include(b => b.Seats)
            .Where(b => b.Route.Source.ToLower() == source.ToLower()
                     && b.Route.Destination.ToLower() == destination.ToLower()
                     && b.IsActive
                     && b.TravelDate >= utcDate && b.TravelDate < utcNext)
            .ToListAsync();

        return Ok(buses.Select(b => new
        {
            b.Id,
            b.RegistrationNumber,
            b.BusName,
            b.BusType,
            Route = new { b.Route.Source, b.Route.Destination },
            BoardingPoint = b.BoardingPoint ?? b.Route.Source,
            DroppingPoint = b.DroppingPoint ?? b.Route.Destination,
            DepartureTime = b.Timing.ToString(@"hh\:mm"),
            ArrivalTime = b.ArrivalTime.HasValue ? b.ArrivalTime.Value.ToString(@"hh\:mm") : null,
            b.TravelDate,
            b.Price,
            AvailableSeats = b.Seats.Count(s => s.IsAvailable && (s.LockedUntil == null || s.LockedUntil <= DateTime.UtcNow)),
            TotalSeats = b.Seats.Count,
            b.IsActive,
            OperatorName = b.BusOperator?.User?.Name ?? "N/A"
        }));
    }

    [HttpPost]
    [Authorize(Policy = "BusOperator")]
    public async Task<ActionResult> CreateBus(CreateBusRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var busOperator = await _context.BusOperators
            .Include(o => o.AssignedRoute)
            .FirstOrDefaultAsync(o => o.UserId == currentUser.Id && o.IsEnabled);

        if (busOperator == null)
            return BadRequest("Bus operator profile not found or not enabled.");

        if (busOperator.AssignedRouteId == null)
            return BadRequest("You do not have an assigned route. Contact admin.");

        var route = await _context.Routes.FindAsync(busOperator.AssignedRouteId.Value);
        if (route == null) return NotFound("Assigned route not found.");

        if (!TimeSpan.TryParse(request.DepartureTime, out var departure))
            return BadRequest("Invalid departure time format. Use HH:mm (e.g. 09:30).");

        TimeSpan? arrival = null;
        if (!string.IsNullOrWhiteSpace(request.ArrivalTime))
        {
            if (!TimeSpan.TryParse(request.ArrivalTime, out var arrivalParsed))
                return BadRequest("Invalid arrival time format. Use HH:mm.");
            arrival = arrivalParsed;
        }

        var bus = new Bus
        {
            RegistrationNumber = request.RegistrationNumber,
            BusName = request.BusName,
            BusType = request.BusType,
            OperatorId = currentUser.Id,
            BusOperatorId = busOperator.Id,
            RouteId = route.Id,
            Timing = departure,
            ArrivalTime = arrival,
            TravelDate = DateTime.SpecifyKind(request.TravelDate.Date, DateTimeKind.Utc),
            SeatLayout = request.SeatLayout,
            TotalSeats = request.TotalSeats,
            Price = request.Price,
            BasePrice = request.Price,
            BoardingPoint = string.IsNullOrWhiteSpace(request.BoardingPoint) ? null : request.BoardingPoint.Trim(),
            DroppingPoint = string.IsNullOrWhiteSpace(request.DroppingPoint) ? null : request.DroppingPoint.Trim(),
            IsActive = true
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        var seatCodes = string.IsNullOrWhiteSpace(request.SeatLayout)
            ? GenerateStandardSeats(request.TotalSeats)
            : ParseSeatCodes(request.SeatLayout);

        foreach (var seatCode in seatCodes)
        {
            _context.Seats.Add(new Seat
            {
                BusId = bus.Id,
                SeatCode = seatCode,
                IsAvailable = true,
                Status = SeatStatus.Available
            });
        }

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBus), new { id = bus.Id }, new { bus.Id, bus.RegistrationNumber });
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBus(Guid id)
    {
        var bus = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.Seats)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null) return NotFound();

        return Ok(new
        {
            bus.Id,
            bus.RegistrationNumber,
            bus.BusName,
            bus.BusType,
            DepartureTime = bus.Timing.ToString(@"hh\:mm"),
            ArrivalTime = bus.ArrivalTime.HasValue ? bus.ArrivalTime.Value.ToString(@"hh\:mm") : null,
            bus.Timing,
            bus.TravelDate,
            bus.Price,
            bus.IsActive,
            Route = bus.Route == null ? null : new { bus.Route.Source, bus.Route.Destination },
            BoardingPoint = bus.BoardingPoint ?? bus.Route?.Source,
            DroppingPoint = bus.DroppingPoint ?? bus.Route?.Destination,
            AllSeats = bus.Seats.Select(s => new
            {
                s.SeatCode,
                s.IsAvailable,
                IsLocked = s.LockedUntil != null && s.LockedUntil > DateTime.UtcNow
            }),
            AvailableSeats = bus.Seats.Count(s => s.IsAvailable && (s.LockedUntil == null || s.LockedUntil <= DateTime.UtcNow)),
            TotalSeats = bus.Seats.Count
        });
    }

    [HttpGet("{id:guid}/stops")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBusStops(Guid id)
    {
        var bus = await _context.Buses
            .Include(b => b.BusOperator).ThenInclude(op => op!.Stops)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bus == null) return NotFound();

        var stops = bus.BusOperator?.Stops ?? new List<OperatorStop>();
        return Ok(new
        {
            Boarding = stops.Where(s => s.Type == StopType.Boarding)
                           .OrderBy(s => s.SortOrder).ThenBy(s => s.StopName)
                           .Select(s => new { s.Id, s.StopName }),
            Dropping = stops.Where(s => s.Type == StopType.Dropping)
                           .OrderBy(s => s.SortOrder).ThenBy(s => s.StopName)
                           .Select(s => new { s.Id, s.StopName })
        });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "BusOperator")]
    public async Task<IActionResult> UpdateBus(Guid id, [FromBody] UpdateBusRequest request)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var operatorProfile = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == currentUser.Id && o.IsEnabled);
        if (operatorProfile == null) return BadRequest("Bus operator profile not found.");

        var bus = await _context.Buses.FirstOrDefaultAsync(b => b.Id == id && b.BusOperatorId == operatorProfile.Id);
        if (bus == null) return NotFound("Bus not found or not owned by this operator.");

        if (!string.IsNullOrWhiteSpace(request.DepartureTime) && TimeSpan.TryParse(request.DepartureTime, out var dep))
            bus.Timing = dep;
        if (!string.IsNullOrWhiteSpace(request.ArrivalTime) && TimeSpan.TryParse(request.ArrivalTime, out var arr))
            bus.ArrivalTime = arr;
        if (request.Price > 0) bus.Price = request.Price;
        if (request.TravelDate != default) bus.TravelDate = DateTime.SpecifyKind(request.TravelDate.Date, DateTimeKind.Utc);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static IEnumerable<string> GenerateStandardSeats(int totalSeats)
    {
        var seats = new List<string>();
        int rows = (int)Math.Ceiling(totalSeats / 4.0);
        int count = 0;
        string[] cols = { "A", "B", "C", "D" };
        for (int row = 1; row <= rows && count < totalSeats; row++)
        {
            foreach (var col in cols)
            {
                if (count >= totalSeats) break;
                seats.Add($"{row}{col}");
                count++;
            }
        }
        return seats;
    }

    private static IEnumerable<string> ParseSeatCodes(string seatLayout)
    {
        if (string.IsNullOrWhiteSpace(seatLayout)) return Array.Empty<string>();

        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(seatLayout);
            if (parsed != null && parsed.Any())
                return parsed.Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
        }
        catch { }

        return seatLayout.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private bool BusExists(Guid id) => _context.Buses.Any(e => e.Id == id);
}

public class UpdateBusRequest
{
    public string? DepartureTime { get; set; }
    public string? ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public DateTime TravelDate { get; set; }
}
