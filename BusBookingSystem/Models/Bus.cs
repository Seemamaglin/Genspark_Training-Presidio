using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Bus
{
    public int Id { get; set; }
    [Required]
    public string RegistrationNumber { get; set; }
    [Required]
    public int BusOperatorId { get; set; }
    public BusOperator BusOperator { get; set; }
    [Required]
    public int RouteId { get; set; }
    public Models.Route Route { get; set; }
    [Required]
    public TimeSpan Timing { get; set; }
    public string SeatLayout { get; set; } // JSON string for seat layout
    [Required]
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}