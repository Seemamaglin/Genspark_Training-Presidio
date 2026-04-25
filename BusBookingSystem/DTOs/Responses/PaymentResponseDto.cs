namespace BusBookingSystem.DTOs.Responses;

public class PaymentResponseDto
{
    public string Status { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
}
