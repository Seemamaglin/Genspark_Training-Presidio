using System;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class Passenger
{
    public Guid Id { get; set; }

    [Required]
    public Guid BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required]
    public Guid SeatId { get; set; }
    public Seat Seat { get; set; } = null!;

    [Required]
    public string SeatCode { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    public int Age { get; set; }

    [Required]
    public string Phone { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string IdProofReference { get; set; } = null!;
}
