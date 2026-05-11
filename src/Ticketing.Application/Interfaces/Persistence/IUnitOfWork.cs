using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IBaseRepository<Ticketing.Domain.Entities.Event> Events { get; }
    IBaseRepository<EventSession> EventSessions { get; }
    IBaseRepository<Seat> Seats { get; }
    IBaseRepository<TicketType> TicketTypes { get; }
    IBaseRepository<Domain.Entities.Booking> Bookings { get; }
    IBaseRepository<BookingTicketSeat> BookingSeats { get; }
    IBaseRepository<BookingTicketType> BookingTicketTypes { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically sets all given seats to Booked if they are Available in this session.
    /// Returns false if any seat could not be claimed (wrong id, session, or not available).
    /// </summary>
    Task<bool> TryAtomicallyBookSeatsAsync(
        IReadOnlyList<int> seatIds,
        int eventSessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically decreases AvailableQuantity for each line if the row is Active, in this session, and has enough stock.
    /// Returns false if any line could not be applied (wrong id, session, inactive, or insufficient quantity).
    /// </summary>
    Task<bool> TryAtomicallyReserveTicketTypesAsync(
        int eventSessionId,
        IReadOnlyList<(int TicketTypeId, int Quantity)> lines,
        CancellationToken cancellationToken = default);

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
