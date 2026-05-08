namespace Ticketing.Application.DTOs.Booking;

public class CreateSeatTypeBookingDto
{
    public int SessionId { get; set; }
    public List<int> SeatIds { get; set; } = [];
}
