using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Payment
{
    public int Id { get; set; }
    [Required]
    public int BookingId { get; set; }
    public Booking Booking { get; set; }
    [Required]
    public decimal Amount { get; set; }
    [Required]
    public string Status { get; set; } // Success, Failure, Pending
    [Required]
    public DateTime PaymentDate { get; set; }
}