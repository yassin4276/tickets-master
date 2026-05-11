using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class UserEventsFilterDto
{
    public string? SearchTerm { get; set; }

    public EventBookingMode? BookingMode { get; set; }

    

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
