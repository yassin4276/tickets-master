namespace Ticketing.Application.DTOs.EventOwner;

public class UpdateTicketTypeDto
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int TotalQuantity { get; set; }
}
