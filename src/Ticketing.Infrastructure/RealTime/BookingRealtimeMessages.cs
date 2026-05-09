namespace Ticketing.Infrastructure.RealTime;

public sealed class SeatBookingRealtimeMessage
{
    public int SessionId { get; init; }
    public List<int> SeatIds { get; init; } = new();
    public string Status { get; init; } = null!;
}

public sealed class TicketTypeAvailabilityItem
{
    public int TicketTypeId { get; init; }
    public int AvailableQuantity { get; init; }
}

public sealed class TicketAvailabilityRealtimeMessage
{
    public int SessionId { get; init; }
    public List<TicketTypeAvailabilityItem> Updates { get; init; } = new();
}
