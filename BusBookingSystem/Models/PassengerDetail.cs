using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class PassengerDetail
{
    public int Id { get; set; }
    [Required]
    public int BookingId { get; set; }
    public Booking Booking { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    [Required]
    public string Email { get; set; }
    [Required]
    public int Age { get; set; }
    [Required]
    public string SeatNumber { get; set; }
    [Required]
    public string Source { get; set; }
    [Required]
    public string Destination { get; set; }
    public string Proof { get; set; }
}