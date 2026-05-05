using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class EventSession
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = null!;
    public SessionStatus Status { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
    public ICollection<TicketType> TicketType { get; set; } = new List<TicketType>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
