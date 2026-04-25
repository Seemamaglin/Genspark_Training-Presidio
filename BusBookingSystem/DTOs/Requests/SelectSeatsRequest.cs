using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class SelectSeatsRequest
{
    [Required]
    public Guid BusId { get; set; }

    [Required]
    [MinLength(1)]
    public List<string> SeatCodes { get; set; } = new();

    public string? ReservationToken { get; set; }
}
