namespace Ticketing.Application.DTOs.Booking;

public class BookingTicketTypeDetailsDto
{
    public int TicketTypeId { get; set; }
    public string Name { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
