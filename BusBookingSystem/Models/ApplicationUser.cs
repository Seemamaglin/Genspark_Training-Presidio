using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BusBookingSystem.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = null!;
    public int Age { get; set; }
    public string? IdProof { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsBusOperatorRequest { get; set; } = false;
    public bool IsApprovedBusOperator { get; set; } = false;

    [NotMapped]
    public string Name
    {
        get => FullName;
        set => FullName = value;
    }

    [NotMapped]
    public string? Proof
    {
        get => IdProof;
        set => IdProof = value;
    }
}