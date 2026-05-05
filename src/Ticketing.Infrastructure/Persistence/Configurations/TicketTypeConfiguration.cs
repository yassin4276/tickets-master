using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class TicketTypeConfiguration : IEntityTypeConfiguration<TicketType>
{
    public void Configure(EntityTypeBuilder<TicketType> builder)
    {
        builder.ToTable("TicketTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(t => t.TotalQuantity)
            .IsRequired();

        builder.Property(t => t.AvailableQuantity)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(t => t.EventSession)
            .WithMany(es => es.TicketType)
            .HasForeignKey(t => t.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Basic indexes
        builder.HasIndex(t => t.EventSessionId);
        builder.HasIndex(t => t.Status);

        // Business constraint
        builder.HasIndex(t => new { t.EventSessionId, t.Name })
            .IsUnique();
    }
}