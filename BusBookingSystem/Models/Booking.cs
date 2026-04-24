using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Booking
{
    public int Id { get; set; }
    [Required]
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    [Required]
    public int BusId { get; set; }
    public Bus Bus { get; set; }
    [Required]
    public DateTime BookingDate { get; set; }
    [Required]
    public string Status { get; set; } // Booked, Cancelled, etc.
    public ICollection<PassengerDetail> PassengerDetails { get; set; } = new List<PassengerDetail>();
    public Payment? Payment { get; set; }
}