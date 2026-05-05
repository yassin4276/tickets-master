using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public SeatStatus Status { get; set; }
    public decimal Price { get; set; }
    public int EventSessionId { get; set; }
    public EventSession EventSession { get; set; } = null!;
    public ICollection<BookingTicketSeat> BookingTicketSeats { get; set; } = new List<BookingTicketSeat>();
}
