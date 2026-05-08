using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class UserEventsFilterDto
{
    public string? SearchTerm { get; set; }

    public int? Id { get; set; }

    public int? OwnerId { get; set; }

    public EventBookingMode? BookingMode { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
