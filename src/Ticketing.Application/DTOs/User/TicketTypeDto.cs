using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.User;

public class TicketTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int AvailableQuantity { get; set; }
    public TicketTypeStatus Status { get; set; }
}
