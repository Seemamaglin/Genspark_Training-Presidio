namespace BusBookingSystem.DTOs.Responses;

public class SeatReservationResponse
{
    public string ReservationToken { get; set; } = null!;
    public DateTime LockedUntil { get; set; }
}
