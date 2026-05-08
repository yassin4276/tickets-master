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
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
