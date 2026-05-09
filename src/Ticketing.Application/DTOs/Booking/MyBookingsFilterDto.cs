using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.Booking;

public class MyBookingsFilterDto
{
    public string? SearchTerm { get; set; }

    public BookingStatus? Status { get; set; }

    public decimal? MinTotalAmount { get; set; }

    public decimal? MaxTotalAmount { get; set; }

    public DateTime? SessionStartFrom { get; set; }

    public DateTime? SessionStartTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
