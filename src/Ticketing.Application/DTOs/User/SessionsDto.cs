using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class SessionsDto
{
    public int Id { get; set; }
    public DateOnly Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Location { get; set; } = null!;
    public SessionStatus Status { get; set; }
}
