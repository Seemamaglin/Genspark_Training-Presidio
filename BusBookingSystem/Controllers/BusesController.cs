using BusBookingSystem.Data;
using BusBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBookingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BusesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Buses/search?source=...&destination=...&date=...
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Bus>>> SearchBuses(string source, string destination, DateTime date)
    {
        var buses = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.BusOperator)
            .Include(b => b.Seats)
            .Where(b => b.Route.Source == source && b.Route.Destination == destination && b.IsActive)
            .ToListAsync();

        return buses;
    }

    // POST: api/Buses
    [HttpPost]
    [Authorize(Policy = "BusOperator")]
    public async Task<ActionResult<Bus>> CreateBus(Bus bus)
    {
        _context.Buses.Add(bus);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBus), new { id = bus.Id }, bus);
    }

    // GET: api/Buses/5
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Bus>> GetBus(int id)
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
    public async Task<IActionResult> UpdateBus(int id, Bus bus)
    {
        if (id != bus.Id)
        {
            return BadRequest();
        }

        _context.Entry(bus).State = EntityState.Modified;

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
    public async Task<IActionResult> DeleteBus(int id)
    {
        var bus = await _context.Buses.FindAsync(id);
        if (bus == null)
        {
            return NotFound();
        }

        _context.Buses.Remove(bus);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool BusExists(int id)
    {
        return _context.Buses.Any(e => e.Id == id);
    }
}