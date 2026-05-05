namespace Ticketing.Application.DTOs.EventOwner;

public class UpdateSeatDto
{
    public string SeatNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
}
