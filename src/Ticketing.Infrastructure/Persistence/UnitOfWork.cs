using Microsoft.EntityFrameworkCore.Storage;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Infrastructure.Persistence.Repositories;

namespace Ticketing.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;

        Events = new BaseRepository<Event>(_context);
        EventSessions = new BaseRepository<EventSession>(_context);
        Seats = new BaseRepository<Seat>(_context);
        TicketTypes = new BaseRepository<TicketType>(_context);
        Bookings = new BaseRepository<Domain.Entities.Booking>(_context);
        BookingSeats = new BaseRepository<BookingTicketSeat>(_context);
        BookingTicketTypes = new BaseRepository<BookingTicketType>(_context);
    }

    public IBaseRepository<Event> Events { get; }
    public IBaseRepository<EventSession> EventSessions { get; }
    public IBaseRepository<Seat> Seats { get; }
    public IBaseRepository<TicketType> TicketTypes { get; }
    public IBaseRepository<Domain.Entities.Booking> Bookings { get; }
    public IBaseRepository<BookingTicketSeat> BookingSeats { get; }
    public IBaseRepository<BookingTicketType> BookingTicketTypes { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }
}