using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Repositories;

namespace Ticketing.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Events = new BaseRepository<Event>(_context);
        EventSessions = new BaseRepository<EventSession>(_context);
        Seats = new BaseRepository<Seat>(_context);
        TicketTypes = new BaseRepository<TicketType>(_context);
        Bookings = new BaseRepository<Booking>(_context);
    }

    public IBaseRepository<Event> Events { get; }
    public IBaseRepository<EventSession> EventSessions { get; }
    public IBaseRepository<Seat> Seats { get; }
    public IBaseRepository<TicketType> TicketTypes { get; }
    public IBaseRepository<Booking> Bookings { get; }
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
