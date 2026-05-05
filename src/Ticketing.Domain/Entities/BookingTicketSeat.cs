namespace Ticketing.Domain.Entities;

public class BookingTicketSeat
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
}
