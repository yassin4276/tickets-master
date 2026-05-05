using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(200);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Basic indexes
        builder.HasIndex(p => p.BookingId)
            .IsUnique();

        builder.HasIndex(p => p.Status);
    }
}