using System;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class SeatLayout
{
    public Guid Id { get; set; }

    [Required]
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = null!;

    [Required]
    public string SeatCode { get; set; } = null!;

    public int SeatRow { get; set; }
    public int SeatColumn { get; set; }
    public string? Deck { get; set; }
    public string? SeatType { get; set; }
}
