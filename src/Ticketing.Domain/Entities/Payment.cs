using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Payment
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public Booking Booking { get; set; } = null!;

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.Simulation;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? TransactionReference { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}