namespace BusBookingSystem.DTOs.Responses;

public class UserBookingResponse
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime BookingDate { get; set; }

    public BusInfo Bus { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public PaymentInfo Payment { get; set; } = null!;
    public List<PassengerInfo> Passengers { get; set; } = new();
}

public class BusInfo
{
    public string RegistrationNumber { get; set; } = null!;
    public DateTime TravelDate { get; set; }
    public TimeSpan Timing { get; set; }
    public RouteInfo Route { get; set; } = null!;
}

public class RouteInfo
{
    public string Source { get; set; } = null!;
    public string Destination { get; set; } = null!;
}

public class PaymentInfo
{
    public string Status { get; set; } = null!;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string TransactionId { get; set; } = null!;
}

public class PassengerInfo
{
    public string Name { get; set; } = null!;
    public string SeatNumber { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public int Age { get; set; }
}