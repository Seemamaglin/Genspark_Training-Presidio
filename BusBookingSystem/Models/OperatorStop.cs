using System.ComponentModel.DataAnnotations;

namespace BusBookingSystem.Models;

public enum StopType { Boarding, Dropping }

public class OperatorStop
{
    public int Id { get; set; }

    [Required]
    public int BusOperatorId { get; set; }
    public BusOperator BusOperator { get; set; } = null!;

    [Required]
    public string StopName { get; set; } = null!;

    [Required]
    public StopType Type { get; set; }

    public int SortOrder { get; set; } = 0;
}
