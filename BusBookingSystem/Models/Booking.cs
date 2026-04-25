using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum BookingStatus
{
    PendingPayment,
    Confirmed,
    Cancelled
}

public class Booking
{
    public Guid Id { get; set; }

    [Required]
    public string BookingReference { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public Guid ScheduleId { get; set; }
    public BusSchedule Schedule { get; set; } = null!;

    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public Guid BusId { get; set; }
    public Bus? Bus { get; set; }

    [Required]
    public DateTime BookingDate { get; set; }
    public ICollection<PassengerDetail> PassengerDetails { get; set; } = new List<PassengerDetail>();
    public Payment? Payment { get; set; }
}