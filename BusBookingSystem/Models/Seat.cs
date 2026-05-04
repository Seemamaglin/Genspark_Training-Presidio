using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum SeatStatus
{
    Available,
    Locked,
    Booked
}

public class Seat
{
    public Guid Id { get; set; }

    public Guid? ScheduleId { get; set; }
    public BusSchedule? Schedule { get; set; }

    [Required]
    public string SeatCode { get; set; } = null!;

    public SeatStatus Status { get; set; } = SeatStatus.Available;
    public string? LockedByUserId { get; set; }
    public DateTime? LockedUntil { get; set; }
    public string? BookedByUserId { get; set; }

    [Required]
    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string? LockedBySessionId { get; set; }
}