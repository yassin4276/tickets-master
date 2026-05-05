using Microsoft.EntityFrameworkCore;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class EventSessionConfiguration : IEntityTypeConfiguration<EventSession>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<EventSession> builder)
    {
        builder.ToTable("EventSessions");
        
        builder.HasKey(es => es.Id);
        
        builder.Property(es => es.StartTime)
            .IsRequired();
        
        builder.Property(es => es.EndTime)
            .IsRequired();
        
        builder.Property(es => es.Location)
            .HasMaxLength(500);
        
        builder.Property(es => es.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.HasOne(es => es.Event)
            .WithMany(e => e.Sessions)
            .HasForeignKey(es => es.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(es => es.Seats)
            .WithOne(s => s.EventSession)
            .HasForeignKey(s => s.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(es => es.TicketType)
            .WithOne(tt => tt.EventSession)
            .HasForeignKey(tt => tt.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(es => es.Bookings)
            .WithOne(b => b.EventSession)
            .HasForeignKey(b => b.EventSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(es => es.EventId);
        builder.HasIndex(es => es.Status);

    }
}
