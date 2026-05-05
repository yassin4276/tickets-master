namespace Ticketing.Application.DTOs.EventOwner;

public class UpdateEventSessionDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = null!;
}
