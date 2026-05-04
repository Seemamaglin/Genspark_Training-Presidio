using BusBookingSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class AddStopRequest
{
    [Required]
    public string StopName { get; set; } = null!;

    [Required]
    public StopType Type { get; set; }

    public int SortOrder { get; set; } = 0;
}
