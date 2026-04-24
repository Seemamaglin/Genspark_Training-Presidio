using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class BusOperator
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    [Required]
    public string Source { get; set; }
    [Required]
    public string Destination { get; set; }
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}