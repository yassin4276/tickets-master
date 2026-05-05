using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class BookingSeatConfiguration : IEntityTypeConfiguration<BookingTicketSeat>
{
    public void Configure(EntityTypeBuilder<BookingTicketSeat> builder)
    {
        builder.ToTable("BookingSeats");

        builder.HasKey(bs => bs.Id);

        builder.Property(bs => bs.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(bs => bs.Booking)
            .WithMany(b => b.BookingTicketSeats)
            .HasForeignKey(bs => bs.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bs => bs.Seat)
            .WithMany(s => s.BookingTicketSeats)
            .HasForeignKey(bs => bs.SeatId)
            .OnDelete(DeleteBehavior.Restrict);

        // Basic indexes
        builder.HasIndex(bs => bs.BookingId);
        builder.HasIndex(bs => bs.SeatId);

        // Business constraint
        builder.HasIndex(bs => new { bs.BookingId, bs.SeatId })
            .IsUnique();
    }
}