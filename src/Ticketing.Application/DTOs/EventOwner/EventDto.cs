using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.EventOwner;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventStatus Status { get; set; }
    public EventBookingMode BookingMode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int OwnerId { get; set; }
}
