using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.Booking;

public class MyBookingsFilterDto
{
    public string? SearchTerm { get; set; }

    public int? Id { get; set; }

    public string? BookingNumber { get; set; }

    public BookingStatus? Status { get; set; }

    public int? EventSessionId { get; set; }

    public string? EventName { get; set; }

    public string? Location { get; set; }

    public decimal? MinTotalAmount { get; set; }

    public decimal? MaxTotalAmount { get; set; }

    public DateTime? CreatedFrom { get; set; }

    public DateTime? CreatedTo { get; set; }

    public DateTime? SessionStartFrom { get; set; }

    public DateTime? SessionStartTo { get; set; }

    public DateTime? SessionEndFrom { get; set; }

    public DateTime? SessionEndTo { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
