namespace BusBookingSystem.DTOs.Responses;

public class PassengerResponseDto
{
    public string Name { get; set; } = null!;
    public string SeatNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int Age { get; set; }
}
