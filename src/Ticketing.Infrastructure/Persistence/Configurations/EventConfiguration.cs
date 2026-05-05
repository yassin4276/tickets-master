using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ticketing.Domain.Entities;

namespace Ticketing.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>   
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(e => e.BookingMode)
            .IsRequired()
            .HasConversion<int>();
        
        builder.Property(e => e.CreatedAt)
            .IsRequired();
        
        builder.Property(e => e.UpdatedAt);
        
        builder.HasMany(e => e.Sessions)
            .WithOne(s => s.Event)
            .HasForeignKey(s => s.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(e => e.OwnerId);
        builder.HasIndex(e => e.Status);
        

    }
}
