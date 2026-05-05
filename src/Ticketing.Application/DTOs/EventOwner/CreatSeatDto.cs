namespace Ticketing.Application.DTOs.EventOwner;

public class CreatSeatDto
{
    public string SeatNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
    public int EventSessionId { get; set; }
}
