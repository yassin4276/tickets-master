namespace Ticketing.Domain.Entities;

public class BookingTicketType
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int TicketTypesId { get; set; }
    public TicketType TicketTypes { get; set; } = null!;
}
