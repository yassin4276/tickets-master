using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class TicketType
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public TicketTypeStatus Status { get; set; }
    public int EventSessionId { get; set; }
    public EventSession EventSession { get; set; } = null!;
    public ICollection<BookingTicketType> BookingTicketTypes { get; set; } = new List<BookingTicketType>();
}
