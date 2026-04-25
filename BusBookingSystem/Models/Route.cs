using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Route
{
    public Guid Id { get; set; }

    [Required]
    public string Source { get; set; } = null!;

    [Required]
    public string Destination { get; set; } = null!;

    public double DistanceKm { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
    public ICollection<OperatorRoute> OperatorRoutes { get; set; } = new List<OperatorRoute>();
}