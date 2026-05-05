using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public BookingStatus Status { get; set; }
    public string BookingNumber { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    //public User User { get; set; } = null!;
    public int EventSessionId { get; set; }
    public EventSession EventSession { get; set; } = null!;
    public ICollection<BookingTicketType> BookingTicketTypes { get; set; } = new List<BookingTicketType>();
    public ICollection<BookingTicketSeat> BookingTicketSeats { get; set; } = new List<BookingTicketSeat>();
    public Payment? Payment { get; set; }

}

