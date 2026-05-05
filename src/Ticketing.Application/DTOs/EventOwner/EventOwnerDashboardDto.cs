using Ticketing.Application.Common.Responses;

namespace Ticketing.Application.DTOs.EventOwner;

public class EventOwnerDashboardDto
{
    public int TotalEvents { get; set; }
    public int DraftEvents { get; set; }
    public int PublishedEvents { get; set; }
    public int UnpublishedEvents { get; set; }
    public int CancelledEvents { get; set; }
    public int SuspendedEvents { get; set; }

    public int TotalSessions { get; set; }
    public int UpcomingSessions { get; set; }
    public int PastSessions { get; set; }


    public int TotalCapacity { get; set; }
    public int SoldQuantity { get; set; }
    public int AvailableQuantity { get; set; }

    public int TotalBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int PendingBookings { get; set; }
    public int CancelledBookings { get; set; }

    public decimal TotalRevenue { get; set; }

    public ApiPagedResponse<EventPerformanceDto> EventsPerformance { get; set; } = new();

    public List<RecentBookingDto> RecentBookings { get; set; } = new();

    public List<DashboardAlertDto>? Alerts { get; set; }
}
