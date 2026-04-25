using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BusBookingSystem.Models;

namespace BusBookingSystem.DTOs.Requests;

public class ConfirmBookingRequest
{
    [Required]
    public Guid BusId { get; set; }

    [Required]
    public string ReservationToken { get; set; } = null!;

    [Required]
    [MinLength(1)]
    public List<PassengerDetail> PassengerDetails { get; set; } = new();

    [Required]
    public string PaymentMethod { get; set; } = null!;
}
