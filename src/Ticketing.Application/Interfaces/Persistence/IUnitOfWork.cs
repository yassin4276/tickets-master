using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    IBaseRepository<Ticketing.Domain.Entities.Event> Events { get; }
    IBaseRepository<EventSession> EventSessions { get; }
    IBaseRepository<Seat> Seats { get; }
    IBaseRepository<TicketType> TicketTypes { get; }
    IBaseRepository<Booking> Bookings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
