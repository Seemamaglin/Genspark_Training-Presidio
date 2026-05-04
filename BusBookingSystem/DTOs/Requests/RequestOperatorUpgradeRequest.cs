using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class RequestOperatorUpgradeRequest
{
    [Required]
    public string Source { get; set; } = null!;

    [Required]
    public string Destination { get; set; } = null!;

    public string? Reason { get; set; }
}
