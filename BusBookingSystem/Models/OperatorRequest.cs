using System;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum OperatorRequestStatus
{
    Pending,
    Approved,
    Rejected
}

public class OperatorRequest
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public OperatorRequestStatus Status { get; set; } = OperatorRequestStatus.Pending;
    public string? RequestReason { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
