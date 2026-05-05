using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.EventOwner;

public class EventPerformanceDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public EventStatus Status { get; set; }
    public EventBookingMode BookingMode { get; set; }

    public int SessionsCount { get; set; }
    public int TotalCapacity { get; set; }
    public int SoldQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int BookingsCount { get; set; }
    public decimal Revenue { get; set; }
}
