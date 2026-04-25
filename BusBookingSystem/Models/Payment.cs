using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum PaymentStatus
{
    Pending,
    Success,
    Failed,
    Refunded
}

public class Payment
{
    public Guid Id { get; set; }

    [Required]
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required]
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionId { get; set; }
    public string? SimulatedOutcome { get; set; }
    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public decimal? RefundAmount { get; set; }
    public string? RefundReason { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = null!;
    [Required]
    public DateTime PaymentDate { get; set; }
}