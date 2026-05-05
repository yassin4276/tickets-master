using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class BookingTicketTypeConfiguration : IEntityTypeConfiguration<BookingTicketType>
{
    public void Configure(EntityTypeBuilder<BookingTicketType> builder)
    {
        builder.ToTable("BookingTicketTypes");

        builder.HasKey(bt => bt.Id);

        builder.Property(bt => bt.Quantity)
            .IsRequired();

        builder.Property(bt => bt.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.HasOne(bt => bt.Booking)
            .WithMany(b => b.BookingTicketTypes)
            .HasForeignKey(bt => bt.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bt => bt.TicketTypes)
            .WithMany(t => t.BookingTicketTypes)
            .HasForeignKey(bt => bt.Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Basic indexes
        builder.HasIndex(bt => bt.BookingId);
        builder.HasIndex(bt => bt.Id);

        // Business constraint
        builder.HasIndex(bt => new { bt.BookingId, bt.Id })
            .IsUnique();
    }
}