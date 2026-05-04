using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum BusStatus
{
    Active,
    Disabled,
    Cancelled
}

public class Bus
{
    public Guid Id { get; set; }

    [Required]
    public string RegistrationNumber { get; set; } = null!;

    public string? BusName { get; set; }
    public string? BusType { get; set; }
    public int TotalSeats { get; set; }

    // Timing is the primary departure time field used throughout
    public TimeSpan Timing { get; set; }
    public TimeSpan? ArrivalTime { get; set; }

    public decimal BasePrice { get; set; }
    public BusStatus Status { get; set; } = BusStatus.Active;
    public string? CancellationReason { get; set; }

    public string? OperatorId { get; set; }
    public ApplicationUser? Operator { get; set; }

    public int? BusOperatorId { get; set; }
    public BusOperator? BusOperator { get; set; }

    [Required]
    public Guid RouteId { get; set; }
    public Route Route { get; set; } = null!;

    [Required]
    public DateTime TravelDate { get; set; }

    [Required]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? SeatLayout { get; set; }
    public bool IsActive { get; set; } = true;

    public string? BoardingPoint { get; set; }
    public string? DroppingPoint { get; set; }

    public ICollection<SeatLayout> SeatLayouts { get; set; } = new List<SeatLayout>();
    public ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
