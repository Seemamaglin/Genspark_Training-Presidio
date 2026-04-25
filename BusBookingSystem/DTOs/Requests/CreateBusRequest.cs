using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class CreateBusRequest
{
    [Required]
    public string RegistrationNumber { get; set; } = null!;

    [Required]
    public TimeSpan Timing { get; set; }

    [Required]
    public DateTime TravelDate { get; set; }

    [Required]
    public string SeatLayout { get; set; } = null!;

    [Required]
    public decimal Price { get; set; }
}
