using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }

    public string BookingNumber { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public BookingStatus Status { get; set; }

    public string EventName { get; set; } = null!;

    public int EventSessionId { get; set; }

    public DateTime SessionStartTime { get; set; }

    public DateTime SessionEndTime { get; set; }

    public string Location { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
