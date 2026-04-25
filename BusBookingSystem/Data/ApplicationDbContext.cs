using BusBookingSystem.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RouteModel = BusBookingSystem.Models.Route;

namespace BusBookingSystem.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BusOperator> BusOperators { get; set; } = null!;
    public DbSet<Bus> Buses { get; set; } = null!;
    public DbSet<RouteModel> Routes { get; set; } = null!;
    public DbSet<Seat> Seats { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<PassengerDetail> PassengerDetails { get; set; } = null!;
    public DbSet<Revenue> Revenues { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
