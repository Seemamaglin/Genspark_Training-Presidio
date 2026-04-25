namespace BusBookingSystem.DTOs.Responses;

public class BookingResponseDto
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public BusResponseDto Bus { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public PaymentResponseDto? Payment { get; set; }
    public IEnumerable<PassengerResponseDto> Passengers { get; set; } = new List<PassengerResponseDto>();
}
