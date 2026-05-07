using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class EventDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventBookingMode BookingMode { get; set; }
    public int OwnerId { get; set; }
}
