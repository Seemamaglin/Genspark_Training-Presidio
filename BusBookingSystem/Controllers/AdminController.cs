using BusBookingSystem.Data;
using BusBookingSystem.Models;
using BusBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteModel = BusBookingSystem.Models.Route;

namespace BusBookingSystem.Controllers;

[ApiController][Route("api/[controller]")][Authorize(Policy = "Admin")]
public class AdminController : ControllerBase {
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly EmailService _emailService;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, EmailService emailService) {
        _context = context; _userManager = userManager; _emailService = emailService;
    }

    [HttpPost("routes")] public async Task<ActionResult<RouteModel>> CreateRoute(RouteModel route) {
        if (await _context.Routes.AnyAsync(r => r.Source == route.Source && r.Destination == route.Destination)) return BadRequest("Route already exists");
        _context.Routes.Add(route); await _context.SaveChangesAsync(); return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, route);
    }
    [HttpGet("routes")] public async Task<ActionResult<IEnumerable<RouteModel>>> GetRoutes() => await _context.Routes.Include(r => r.Buses).ToListAsync();
    [HttpGet("routes/{id}")]
    public async Task<ActionResult<RouteModel>> GetRoute(int id)
    {
        var route = await _context.Routes.Include(r => r.Buses).FirstOrDefaultAsync(r => r.Id == id);
        if (route == null) return NotFound();
        return route;
    }
    [HttpPut("routes/{id}")] public async Task<IActionResult> UpdateRoute(int id, RouteModel route) { if (id != route.Id) return BadRequest(); _context.Entry(route).State = EntityState.Modified; try { await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!_context.Routes.Any(r => r.Id == id)) return NotFound(); throw; } return NoContent(); }
    [HttpDelete("routes/{id}")] public async Task<IActionResult> DeleteRoute(int id) { var route = await _context.Routes.FindAsync(id); if (route == null) return NotFound(); _context.Routes.Remove(route); await _context.SaveChangesAsync(); return NoContent(); }

    [HttpPost("operators/{userId}/approve")]
    public async Task<IActionResult> ApproveBusOperator(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        var busOperator = new BusOperator { UserId = userId, User = user, Source = "", Destination = "" };
        _context.BusOperators.Add(busOperator);
        await _userManager.AddToRoleAsync(user, "BusOperator");
        await _context.SaveChangesAsync();
        if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Name))
            await _emailService.SendApprovalEmailAsync(user.Email, user.Name, "approved");
        return Ok("Approved");
    }
    [HttpPost("operators/{userId}/reject")]
    public async Task<IActionResult> RejectBusOperator(string userId, [FromBody] string reason)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();
        if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(user.Name))
            await _emailService.SendApprovalEmailAsync(user.Email, user.Name, "rejected", reason);
        return Ok("Rejected");
    }
    [HttpPost("operators/{id}/disable")] public async Task<IActionResult> DisableOperator(int id) { var busOperator = await _context.BusOperators.Include(o => o.Buses).FirstOrDefaultAsync(o => o.Id == id); if (busOperator == null) return NotFound(); foreach (var bus in busOperator.Buses) bus.IsActive = false; var user = await _userManager.FindByIdAsync(busOperator.UserId); if (user != null) { await _userManager.RemoveFromRoleAsync(user, "BusOperator"); user.LockoutEnd = DateTime.MaxValue; await _userManager.UpdateAsync(user); } await _context.SaveChangesAsync(); return Ok("Disabled"); }
    [HttpPost("operators/{id}/enable")] public async Task<IActionResult> EnableOperator(int id) { var busOperator = await _context.BusOperators.FirstOrDefaultAsync(o => o.Id == id); if (busOperator == null) return NotFound(); var user = await _userManager.FindByIdAsync(busOperator.UserId); if (user != null) { await _userManager.AddToRoleAsync(user, "BusOperator"); user.LockoutEnd = null; await _userManager.UpdateAsync(user); } await _context.SaveChangesAsync(); return Ok("Enabled"); }
    [HttpPut("operators/{id}/route")] public async Task<IActionResult> AssignRouteToOperator(int id, [FromBody] int routeId) { var busOperator = await _context.BusOperators.FindAsync(id); if (busOperator == null) return NotFound(); var route = await _context.Routes.FindAsync(routeId); if (route == null) return NotFound("Route not found"); busOperator.Source = route.Source; busOperator.Destination = route.Destination; _context.Entry(busOperator).State = EntityState.Modified; await _context.SaveChangesAsync(); return Ok("Route assigned"); }

    [HttpGet("revenue")]
    public async Task<ActionResult<object>> GetTotalRevenue()
    {
        var total = await _context.Payments.Where(p => p.Status == "Success").SumAsync(p => p.Amount);
        var byOp = await _context.BusOperators.Include(o => o.Buses).ThenInclude(b => b.Bookings).ThenInclude(bo => bo.Payment)
            .Select(o => new {
                OpId = o.Id,
                OpName = o.User != null ? o.User.Name : string.Empty,
                Revenue = o.Buses.SelectMany(b => b.Bookings ?? new List<Booking>())
                    .Where(bo => bo.Payment != null && bo.Payment.Status == "Success")
                    .Sum(bo => bo.Payment != null ? bo.Payment.Amount : 0)
            }).ToListAsync();
        return Ok(new { Total = total, ByOperator = byOp });
    }
    [HttpGet("revenue/bus/{busId}")]
    public async Task<ActionResult<object>> GetBusRevenue(int busId)
    {
        var bus = await _context.Buses.Include(b => b.Bookings).ThenInclude(bo => bo.Payment).FirstOrDefaultAsync(b => b.Id == busId);
        if (bus == null) return NotFound();
        var revenue = bus.Bookings?.Where(bo => bo.Payment != null && bo.Payment.Status == "Success").Sum(bo => bo.Payment?.Amount ?? 0) ?? 0;
        return Ok(new { BusId = busId, Revenue = revenue });
    }
    [HttpGet("revenue/operator/{operatorId}")]
    public async Task<ActionResult<object>> GetOperatorRevenue(int operatorId)
    {
        var op = await _context.BusOperators.Include(o => o.Buses).ThenInclude(b => b.Bookings).ThenInclude(bo => bo.Payment).FirstOrDefaultAsync(o => o.Id == operatorId);
        if (op == null) return NotFound();
        var revenue = op.Buses?.SelectMany(b => b.Bookings ?? new List<Booking>()).Where(bo => bo.Payment != null && bo.Payment.Status == "Success").Sum(bo => bo.Payment?.Amount ?? 0) ?? 0;
        return Ok(new { OpId = operatorId, OpName = op.User != null ? op.User.Name : string.Empty, Revenue = revenue });
    }

    [HttpPost("buses/{busId}/cancel")]
    public async Task<IActionResult> CancelBus(int busId, [FromBody] CancelBusRequest request)
    {
        var bus = await _context.Buses.Include(b => b.Bookings).ThenInclude(bo => bo.User).FirstOrDefaultAsync(b => b.Id == busId);
        if (bus == null) return NotFound();
        bus.IsActive = false;
        var bookings = bus.Bookings?.Where(b => b.Status == "Booked").ToList() ?? new List<Booking>();
        foreach (var booking in bookings)
        {
            booking.Status = "Cancelled";
            if (booking.Payment != null) booking.Payment.Status = "Refunded";
            if (booking.User != null && !string.IsNullOrEmpty(booking.User.Email) && !string.IsNullOrEmpty(booking.User.Name))
                await _emailService.SendCancellationEmailAsync(
                    booking.User.Email,
                    booking.User.Name,
                    bus.RegistrationNumber,
                    request.CancellationReason ?? string.Empty,
                    booking.Payment?.Amount ?? 0);
        }
        var op = await _context.BusOperators.Include(o => o.User).FirstOrDefaultAsync(o => o.Id == bus.BusOperatorId);
        if (op != null && op.User != null && !string.IsNullOrEmpty(op.User.Email) && !string.IsNullOrEmpty(op.User.Name))
            await _emailService.SendOperatorCancellationEmailAsync(
                op.User.Email,
                op.User.Name,
                bus.RegistrationNumber,
                request.CancellationReason ?? string.Empty,
                bookings.Count,
                bookings.Sum(b => b.Payment?.Amount ?? 0));
        await _context.SaveChangesAsync();
        return Ok("Cancelled");
    }
    [HttpGet("buses")] public async Task<ActionResult<IEnumerable<Bus>>> GetAllBuses() => await _context.Buses.Include(b => b.Route).Include(b => b.BusOperator).ToListAsync();
    [HttpGet("buses/inactive")] public async Task<ActionResult<IEnumerable<Bus>>> GetInactiveBuses() => await _context.Buses.Include(b => b.Route).Include(b => b.BusOperator).Where(b => !b.IsActive).ToListAsync();
    [HttpGet("users")] public async Task<ActionResult<IEnumerable<object>>> GetAllUsers() { var users = await _userManager.Users.ToListAsync(); var result = new List<object>(); foreach (var user in users) { var roles = await _userManager.GetRolesAsync(user); result.Add(new { user.Id, user.Email, user.Name, Roles = roles }); } return result; }
}

public class CancelBusRequest { public string? CancellationReason { get; set; } }
