using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class PassengerRequest
{
    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public string PhoneNumber { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [Range(1, 120)]
    public int Age { get; set; }

    [Required]
    public string SeatNumber { get; set; } = null!;
}
