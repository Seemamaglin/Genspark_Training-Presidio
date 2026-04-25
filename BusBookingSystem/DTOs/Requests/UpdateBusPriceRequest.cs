using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.DTOs.Requests;

public class UpdateBusPriceRequest
{
    [Required]
    public decimal Price { get; set; }
}
