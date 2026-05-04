using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class ConfirmBookingRequest
{
    [Required]
    public Guid BusId { get; set; }

    [Required]
    public string ReservationToken { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public List<PassengerRequest> PassengerDetails { get; set; } = new();

    [Required]
    public string PaymentMethod { get; set; } = null!;

    public string? PickupStop { get; set; }
    public string? DroppingStop { get; set; }
}
