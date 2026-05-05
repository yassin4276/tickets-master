using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.EventOwner;

public class UpdateEventDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventBookingMode BookingMode { get; set; }
}
