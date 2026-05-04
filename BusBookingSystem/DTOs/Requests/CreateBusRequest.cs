using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class CreateBusRequest
{
    [Required]
    public string RegistrationNumber { get; set; } = null!;

    public string? BusName { get; set; }

    public string? BusType { get; set; } // "AC Seater", "Non-AC Seater", "Sleeper", "Semi-Sleeper"

    [Required]
    public string DepartureTime { get; set; } = "09:00"; // "HH:mm"

    public string? ArrivalTime { get; set; } // "HH:mm"

    [Required]
    public DateTime TravelDate { get; set; }

    [Range(10, 60)]
    public int TotalSeats { get; set; } = 40;

    [Required]
    [Range(1, 100000)]
    public decimal Price { get; set; }

    // Optional - if null, auto-generated from TotalSeats as standard 2+2 layout
    public string? SeatLayout { get; set; }

    // Required for admin bus creation, ignored for operators (they use their assigned route)
    public Guid? RouteId { get; set; }

    // Optional operator assignment for admin bus creation
    public int? OperatorId { get; set; }

    // Specific stop within the source city where passengers board
    public string? BoardingPoint { get; set; }

    // Specific stop within the destination city where passengers alight
    public string? DroppingPoint { get; set; }
}
