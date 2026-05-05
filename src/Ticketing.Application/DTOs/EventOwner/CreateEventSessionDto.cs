
namespace Ticketing.Application.DTOs.EventOwner;

public class CreateEventSessionDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Location { get; set; } = null!;
    public int EventId { get; set; }
}
