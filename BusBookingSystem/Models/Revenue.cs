using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Revenue
{
    public Guid Id { get; set; }

    [Required]
    public string OperatorId { get; set; } = null!;
    public ApplicationUser? Operator { get; set; }

    [Required]
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = null!;

    [Required]
    public Guid ScheduleId { get; set; }
    public BusSchedule Schedule { get; set; } = null!;

    [Required]
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required]
    public decimal Amount { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}