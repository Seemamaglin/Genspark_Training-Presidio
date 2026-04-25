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
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public decimal BasePrice { get; set; }
    public BusStatus Status { get; set; } = BusStatus.Active;
    public string? CancellationReason { get; set; }

    [Required]
    public string OperatorId { get; set; } = null!;
    public ApplicationUser Operator { get; set; } = null!;

    [Required]
    public int BusOperatorId { get; set; }
    public BusOperator BusOperator { get; set; } = null!;

    [Required]
    public Guid RouteId { get; set; }
    public Route Route { get; set; } = null!;

    [Required]
    public TimeSpan Timing { get; set; }
    [Required]
    public DateTime TravelDate { get; set; }
    [Required]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string SeatLayout { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<SeatLayout> SeatLayouts { get; set; } = new List<SeatLayout>();
    public ICollection<BusSchedule> BusSchedules { get; set; } = new List<BusSchedule>();
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}