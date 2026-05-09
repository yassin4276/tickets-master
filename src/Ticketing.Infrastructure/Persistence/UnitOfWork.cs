using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
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

    public async Task<bool> TryAtomicallyBookSeatsAsync(
        IReadOnlyList<int> seatIds,
        int eventSessionId,
        CancellationToken cancellationToken = default)
    {
        var distinctIds = seatIds.Distinct().ToList();
        if (distinctIds.Count == 0)
            return false;

        var updatedCount = await _context.Seats
            .Where(s =>
                distinctIds.Contains(s.Id) &&
                s.EventSessionId == eventSessionId &&
                s.Status == SeatStatus.Available)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.Status, SeatStatus.Booked),
                cancellationToken);

        return updatedCount == distinctIds.Count;
    }

    public async Task<bool> TryAtomicallyReserveTicketTypesAsync(
        int eventSessionId,
        IReadOnlyList<(int TicketTypeId, int Quantity)> lines,
        CancellationToken cancellationToken = default)
    {
        foreach (var (ticketTypeId, quantity) in lines)
        {
            if (quantity <= 0)
                return false;

            var updated = await _context.TicketTypes
                .Where(t =>
                    t.Id == ticketTypeId &&
                    t.EventSessionId == eventSessionId &&
                    t.Status == TicketTypeStatus.Active &&
                    t.AvailableQuantity >= quantity)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        t => t.AvailableQuantity,
                        t => t.AvailableQuantity - quantity),
                    cancellationToken);

            if (updated != 1)
                return false;
        }

        return true;
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