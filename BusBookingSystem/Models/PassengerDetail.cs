using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class PassengerDetail
{
    public int Id { get; set; }
    [Required]
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public int Age { get; set; }
    [Required]
    public string SeatNumber { get; set; } = null!;
    [Required]
    public string Source { get; set; } = null!;
    [Required]
    public string Destination { get; set; } = null!;
    public string? Proof { get; set; }
}