using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.EventOwner;

public class EventOwnerEventsFilterDto
{
    public string? SearchTerm { get; set; }

    public EventStatus? Status { get; set; }

    public EventBookingMode? BookingMode { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
