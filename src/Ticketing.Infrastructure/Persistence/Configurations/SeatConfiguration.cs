using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class SeatConfiguration : IEntityTypeConfiguration<Seat>
{
    public void Configure(EntityTypeBuilder<Seat> builder)
    {
        builder.ToTable("Seats");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SeatNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(s => s.EventSession)
            .WithMany(es => es.Seats)
            .HasForeignKey(s => s.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Basic indexes
        builder.HasIndex(s => s.EventSessionId);
        builder.HasIndex(s => s.Status);

        // Business constraint
        builder.HasIndex(s => new { s.EventSessionId, s.SeatNumber })
            .IsUnique();
    }
}