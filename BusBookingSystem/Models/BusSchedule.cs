using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public class BusSchedule
{
    public Guid Id { get; set; }

    [Required]
    public Guid BusId { get; set; }
    public Bus Bus { get; set; } = null!;

    public DateOnly TravelDate { get; set; }
    public bool IsCancelled { get; set; }
    public string? CancellationReason { get; set; }

    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
