using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.Booking;

public class BookingDetailsDto
{
    public int Id { get; set; }
    public string BookingNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; }

    public int EventSessionId { get; set; }
    public string EventName { get; set; } = null!;
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionEndTime { get; set; }
    public string Location { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public List<BookingSeatDetailsDto> Seats { get; set; } = new();
    public List<BookingTicketTypeDetailsDto> TicketTypes { get; set; } = new();
}
