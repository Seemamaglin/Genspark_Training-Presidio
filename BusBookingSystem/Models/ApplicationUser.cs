using Microsoft.AspNetCore.Identity;

namespace BusBookingSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public int Age { get; set; }
    public string? Proof { get; set; } // ID reference or dummy field
    public bool IsBusOperatorRequest { get; set; } = false;
    public bool IsApprovedBusOperator { get; set; } = false;
}