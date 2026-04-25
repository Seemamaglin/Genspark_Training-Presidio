using System.Collections.Generic;

namespace BusBookingSystem.DTOs.Responses;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public IList<string> Roles { get; set; } = new List<string>();
}
