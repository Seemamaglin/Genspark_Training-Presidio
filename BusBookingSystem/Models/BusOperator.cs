using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class BusOperator
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
    [Required]
    public string Source { get; set; } = null!;
    [Required]
    public string Destination { get; set; } = null!;
    public Guid? AssignedRouteId { get; set; }
    public Route? AssignedRoute { get; set; }
    public bool IsEnabled { get; set; } = true;
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}