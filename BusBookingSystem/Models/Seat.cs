using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Seat
{
    public int Id { get; set; }
    [Required]
    public int BusId { get; set; }
    public Bus Bus { get; set; }
    [Required]
    public string SeatCode { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime? LockedUntil { get; set; } // for temporary lock
    public string? LockedByUserId { get; set; }
}