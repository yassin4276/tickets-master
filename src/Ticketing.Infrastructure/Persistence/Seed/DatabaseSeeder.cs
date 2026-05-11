using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.Identity;

namespace Ticketing.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedTestDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Keep this idempotent: only seed when target tables are empty.
        var hasAny =
            await db.Events.AsNoTracking().AnyAsync() ||
            await db.EventSessions.AsNoTracking().AnyAsync() ||
            await db.Seats.AsNoTracking().AnyAsync() ||
            await db.TicketTypes.AsNoTracking().AnyAsync();

        if (hasAny)
            return;

        var owner = await userManager.FindByEmailAsync("owner@test.local");
        if (owner is null)
            throw new InvalidOperationException("Seed user 'owner@test.local' was not found. Ensure IdentitySeeder.SeedUsersAsync ran first.");

        var now = DateTime.UtcNow;

        var event1 = new Event
        {
            Name = "Test Event 1",
            Description = "Seeded test event (1)",
            Status = EventStatus.Published,
            BookingMode = EventBookingMode.Seats,
            CreatedAt = now,
            UpdatedAt = null,
            OwnerId = owner.Id
        };

        var event2 = new Event
        {
            Name = "Test Event 2",
            Description = "Seeded test event (2)",
            Status = EventStatus.Published,
            BookingMode = EventBookingMode.Tickets,
            CreatedAt = now,
            UpdatedAt = null,
            OwnerId = owner.Id
        };

        db.Events.AddRange(event1, event2);
        await db.SaveChangesAsync();

        var session1 = new EventSession
        {
            EventId = event1.Id,
            StartTime = now.AddDays(1),
            EndTime = now.AddDays(1).AddHours(2),
            Location = "Test Location A",
            Status = SessionStatus.Scheduled
        };

        var session2 = new EventSession
        {
            EventId = event2.Id,
            StartTime = now.AddDays(2),
            EndTime = now.AddDays(2).AddHours(3),
            Location = "Test Location B",
            Status = SessionStatus.Scheduled
        };

        db.EventSessions.AddRange(session1, session2);
        await db.SaveChangesAsync();

        var seat1 = new Seat
        {
            EventSessionId = session1.Id,
            SeatNumber = "A-1",
            Type = "Standard",
            Status = SeatStatus.Available,
            Price = 25m
        };

        var seat2 = new Seat
        {
            EventSessionId = session1.Id,
            SeatNumber = "B-1",
            Type = "VIP",
            Status = SeatStatus.Available,
            Price = 50m
        };

        db.Seats.AddRange(seat1, seat2);

        var type1 = new TicketType
        {
            EventSessionId = session2.Id,
            Name = "General Admission",
            Price = 20m,
            TotalQuantity = 100,
            AvailableQuantity = 100,
            Status = TicketTypeStatus.Active
        };

        var type2 = new TicketType
        {
            EventSessionId = session2.Id,
            Name = "VIP Pass",
            Price = 60m,
            TotalQuantity = 20,
            AvailableQuantity = 20,
            Status = TicketTypeStatus.Active
        };

        db.TicketTypes.AddRange(type1, type2);

        await db.SaveChangesAsync();
    }
}
