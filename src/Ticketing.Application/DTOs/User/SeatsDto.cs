using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class SeatsDto
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = null!;
    public string Type { get; set; } = null!;
    public SeatStatus Status { get; set; }
    public decimal Price { get; set; }
}
