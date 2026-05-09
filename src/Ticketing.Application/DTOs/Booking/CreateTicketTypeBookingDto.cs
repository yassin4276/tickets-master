namespace Ticketing.Application.DTOs.Booking;

public class CreateTicketTypeBookingDto
{
    public int SessionId { get; set; }
    public List<CreateTicketTypeBookingItemDto> TicketTypes { get; set; } = new List<CreateTicketTypeBookingItemDto>();
}
