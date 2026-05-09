namespace Ticketing.Application.DTOs.Booking;

public class BookingSeatDetailsDto
{
    public int SeatId { get; set; }
    public string SeatNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public decimal Price { get; set; }
}
