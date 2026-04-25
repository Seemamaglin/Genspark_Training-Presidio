using System;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class OperatorRoute
{
    public Guid Id { get; set; }

    [Required]
    public string OperatorId { get; set; } = null!;
    public ApplicationUser Operator { get; set; } = null!;

    [Required]
    public Guid RouteId { get; set; }
    public Route Route { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
