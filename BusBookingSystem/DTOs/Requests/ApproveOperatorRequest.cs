using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class ApproveOperatorRequest
{
    [Required]
    public Guid RouteId { get; set; }
}
