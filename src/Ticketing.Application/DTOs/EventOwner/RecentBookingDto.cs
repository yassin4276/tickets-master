using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.EventOwner;

public class RecentBookingDto
{
    public int BookingId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime SessionStartDate { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
}
