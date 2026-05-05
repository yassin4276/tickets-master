using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventStatus Status { get; set; }
    public EventBookingMode BookingMode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int OwnerId { get; set; }
    public ICollection<EventSession> Sessions { get; set; } = new List<EventSession>();
}
