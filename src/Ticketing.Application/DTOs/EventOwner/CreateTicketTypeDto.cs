namespace Ticketing.Application.DTOs.EventOwner;

public class CreateTicketTypeDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
    public int EventSessionId { get; set; }
}
