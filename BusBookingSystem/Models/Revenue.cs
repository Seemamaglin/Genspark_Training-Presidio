using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Revenue
{
    public int Id { get; set; }
    [Required]
    public int BusId { get; set; }
    public Bus Bus { get; set; }
    [Required]
    public decimal TotalRevenue { get; set; }
    [Required]
    public DateTime Date { get; set; }
}