namespace Ticketing.Application.DTOs.EventOwner;

public class EventOwnerDashboardFilterDto
{
    /// <summary>1-based page index for the events performance list.</summary>
    public int EventPerformancePageNumber { get; set; } = 1;

    /// <summary>Page size for the events performance list (capped in the service).</summary>
    public int EventPerformancePageSize { get; set; } = 10;
}
