namespace BusBookingSystem.DTOs.Responses;

public class BusResponseDto
{
    public string RegistrationNumber { get; set; } = null!;
    public DateTime TravelDate { get; set; }
    public TimeSpan Timing { get; set; }
    public RouteResponseDto Route { get; set; } = null!;
}
