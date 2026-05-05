using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.EventOwner;
using Ticketing.Application.Interfaces.EventOwner;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.EventOwner;

public class EventOwnerService : IEventOwnerService
{
    private readonly IUnitOfWork _unitOfWork;

    public EventOwnerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool IsSuccess, EventDto? Data, string? ErrorMessage)> CreateEventAsync(int ownerId, CreateEventDto createEventDto, CancellationToken cancellationToken = default)
    {
        var newEvent = new Ticketing.Domain.Entities.Event
        {
            Name = createEventDto.Name,
            Description = createEventDto.Description,
            BookingMode = createEventDto.BookingMode,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            OwnerId = ownerId,
            Status = EventStatus.Draft,
        };
        await _unitOfWork.Events.AddAsync(newEvent, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, new EventDto
        {
            Id = newEvent.Id,
            Name = newEvent.Name,
            Description = newEvent.Description,
            BookingMode = newEvent.BookingMode,
            Status = newEvent.Status,
            CreatedAt = newEvent.CreatedAt,
            UpdatedAt = newEvent.UpdatedAt,
        }, null);
    }

    public async Task<(bool IsSuccess, ApiPagedResponse<EventDto>? Events, string? ErrorMessage)> GetMyEventsAsync(int ownerId, EventOwnerEventsFilterDto filter, CancellationToken cancellationToken = default)
    {
        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
        pageSize = Math.Min(pageSize, 50);

        var query = _unitOfWork.Events.GetAll().AsNoTracking().Where(e => e.OwnerId == ownerId);

        if (!string.IsNullOrEmpty(filter.SearchTerm))
        {
            query = query.Where(e => e.Name.Contains(filter.SearchTerm));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(e => e.Status == filter.Status.Value);
        }

        if (filter.BookingMode.HasValue)
        {
            query = query.Where(e => e.BookingMode == filter.BookingMode.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= filter.CreatedTo.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                Status = e.Status,
                BookingMode = e.BookingMode
            })
            .ToListAsync(cancellationToken);

        var response = new ApiPagedResponse<EventDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = events
        };

        return (true, response, null);
    }

    public async Task<(bool IsSuccess, EventDto? Event, string? ErrorMessage)> GetEventByIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var evententity = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (evententity == null)
        {
            return (false, null, "Event not found");
        }
        return (true, new EventDto
        {
            Id = evententity.Id,
            Name = evententity.Name,
            Description = evententity.Description,
            Status = evententity.Status,
            BookingMode = evententity.BookingMode,
        }, null);
    }
        

    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateEventAsync(int eventId, UpdateEventDto updateEventDto, CancellationToken cancellationToken = default)
    {
        var existingEvent = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (existingEvent == null)
        {
            return (false, "Event not found");
        }
        existingEvent.Name = updateEventDto.Name;
        existingEvent.Description = updateEventDto.Description;
        existingEvent.BookingMode = updateEventDto.BookingMode;
        existingEvent.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Events.Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CreateEventSessionAsync(int eventId, int ownerId, CreateEventSessionDto createEventSessionDto, CancellationToken cancellationToken = default)
    {
        var existingEvent = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (existingEvent == null)
        {
            return (false, "Event not found");
        }
        if (existingEvent.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        if (createEventSessionDto.StartTime >= createEventSessionDto.EndTime)
        {
            return (false, "Start time must be before end time");
        }
        var newEventSession = new EventSession
        {
            StartTime = createEventSessionDto.StartTime,
            EndTime = createEventSessionDto.EndTime,
            Location = createEventSessionDto.Location,
            Status = SessionStatus.Scheduled,
            EventId = eventId,
        };
        await _unitOfWork.EventSessions.AddAsync(newEventSession, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateEventSessionAsync(int sessionId, int ownerId, UpdateEventSessionDto dto, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.EventSessions.GetByCondition(s => s.Id == sessionId)
            .Include(s => s.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (session == null)
        {
            return (false, "Event session not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(session.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        if (dto.StartTime >= dto.EndTime)
        {
            return (false, "Start time must be before end time");
        }
        session.StartTime = dto.StartTime;
        session.EndTime = dto.EndTime;
        session.Location = dto.Location;
        _unitOfWork.EventSessions.Update(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteEventSessionAsync(int sessionId, int ownerId, CancellationToken cancellationToken = default)
    {
        var session = await _unitOfWork.EventSessions.GetByCondition(s => s.Id == sessionId)
            .Include(s => s.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (session == null)
        {
            return (false, "Event session not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(session.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        var hasBookings = await _unitOfWork.Bookings.GetAll()
            .AnyAsync(b => b.EventSessionId == sessionId, cancellationToken);
        if (hasBookings)
        {
            return (false, "Cannot delete a session that has bookings");
        }
        _unitOfWork.EventSessions.Delete(session);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CreateSeatAsync(int eventSessionId, int ownerId, CreatSeatDto createSeatDto, CancellationToken cancellationToken = default)
    {
        var existingEventSession = await _unitOfWork.EventSessions.GetByCondition(e => e.Id == eventSessionId)
            .Include(e => e.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingEventSession == null)
        {
            return (false, "Event session not found");
        }
        if (existingEventSession.Event.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        if (existingEventSession.Event.BookingMode != EventBookingMode.Seats)
        {
            return (false, "Seats can only be added when the event uses seat booking.");
        }
        var newSeat = new Seat
        {
            SeatNumber = createSeatDto.SeatNumber,
            Type = createSeatDto.Type,
            Price = createSeatDto.Price,
            Status = SeatStatus.Available,
            EventSessionId = eventSessionId,
        };
        await _unitOfWork.Seats.AddAsync(newSeat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateSeatAsync(int seatId, int ownerId, UpdateSeatDto dto, CancellationToken cancellationToken = default)
    {
        var seat = await _unitOfWork.Seats.GetByCondition(s => s.Id == seatId)
            .Include(s => s.EventSession)
            .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (seat == null)
        {
            return (false, "Seat not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(seat.EventSession.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        seat.SeatNumber = dto.SeatNumber;
        seat.Type = dto.Type;
        seat.Price = dto.Price;
        _unitOfWork.Seats.Update(seat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteSeatAsync(int seatId, int ownerId, CancellationToken cancellationToken = default)
    {
        var seat = await _unitOfWork.Seats.GetByCondition(s => s.Id == seatId)
            .Include(s => s.EventSession)
            .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (seat == null)
        {
            return (false, "Seat not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(seat.EventSession.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        var hasBookingLines = await _unitOfWork.Bookings.GetAll()
            .AnyAsync(b => b.BookingTicketSeats.Any(bs => bs.SeatId == seatId), cancellationToken);
        if (hasBookingLines)
        {
            return (false, "Cannot delete a seat that is referenced by a booking");
        }
        _unitOfWork.Seats.Delete(seat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CreateTicketTypeAsync(int eventSessionId, int ownerId, CreateTicketTypeDto createTicketTypeDto, CancellationToken cancellationToken = default)
    {
        var existingEventSession = await _unitOfWork.EventSessions.GetByCondition(e => e.Id == eventSessionId)
            .Include(e => e.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (existingEventSession == null)
        {
            return (false, "Event session not found");
        }
        if (existingEventSession.Event.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        if (existingEventSession.Event.BookingMode != EventBookingMode.Tickets)
        {
            return (false, "Ticket types can only be added when the event uses ticket booking.");
        }
        var newTicketType = new TicketType
        {
            Name = createTicketTypeDto.Name,
            Price = createTicketTypeDto.Price,
            TotalQuantity = createTicketTypeDto.TotalQuantity,
            AvailableQuantity = createTicketTypeDto.TotalQuantity,
            Status = TicketTypeStatus.Active,
            EventSessionId = eventSessionId,
        };
        await _unitOfWork.TicketTypes.AddAsync(newTicketType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> UpdateTicketTypeAsync(int ticketTypeId, int ownerId, UpdateTicketTypeDto dto, CancellationToken cancellationToken = default)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByCondition(t => t.Id == ticketTypeId)
            .Include(t => t.EventSession)
            .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (ticketType == null)
        {
            return (false, "Ticket type not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(ticketType.EventSession.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        var soldOrReserved = ticketType.TotalQuantity - ticketType.AvailableQuantity;
        if (dto.TotalQuantity < soldOrReserved)
        {
            return (false, "Total quantity cannot be less than the quantity already reserved or sold");
        }
        ticketType.Name = dto.Name;
        ticketType.Price = dto.Price;
        ticketType.TotalQuantity = dto.TotalQuantity;
        ticketType.AvailableQuantity = dto.TotalQuantity - soldOrReserved;
        _unitOfWork.TicketTypes.Update(ticketType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteTicketTypeAsync(int ticketTypeId, int ownerId, CancellationToken cancellationToken = default)
    {
        var ticketType = await _unitOfWork.TicketTypes.GetByCondition(t => t.Id == ticketTypeId)
            .Include(t => t.EventSession)
            .ThenInclude(es => es.Event)
            .FirstOrDefaultAsync(cancellationToken);
        if (ticketType == null)
        {
            return (false, "Ticket type not found");
        }
        var guard = await EnsureOwnerDraftNoConfirmedBookingsAsync(ticketType.EventSession.EventId, ownerId, cancellationToken);
        if (!guard.IsSuccess)
        {
            return guard;
        }
        var hasBookingLines = await _unitOfWork.Bookings.GetAll()
            .AnyAsync(b => b.BookingTicketTypes.Any(tt => tt.TicketTypesId == ticketTypeId), cancellationToken);
        if (hasBookingLines)
        {
            return (false, "Cannot delete a ticket type that is referenced by a booking");
        }
        _unitOfWork.TicketTypes.Delete(ticketType);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> PublishEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default)
    {
        var existingEvent = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (existingEvent == null)
        {
            return (false, "Event not found");
        }
        if (existingEvent.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }

        var sessionCount = await _unitOfWork.EventSessions.GetAll()
            .CountAsync(s => s.EventId == eventId, cancellationToken);
        if (sessionCount == 0)
        {
            return (false, "Add at least one session before publishing the event.");
        }

        if (existingEvent.BookingMode == EventBookingMode.Seats)
        {
            var hasSeat = await _unitOfWork.Seats.GetAll()
                .AnyAsync(s => s.EventSession.EventId == eventId, cancellationToken);
            if (!hasSeat)
            {
                return (false, "Add at least one seat before publishing a seat-based event.");
            }
        }
        else if (existingEvent.BookingMode == EventBookingMode.Tickets)
        {
            var hasTicketType = await _unitOfWork.TicketTypes.GetAll()
                .AnyAsync(t => t.EventSession.EventId == eventId, cancellationToken);
            if (!hasTicketType)
            {
                return (false, "Add at least one ticket type before publishing a ticket-based event.");
            }
        }

        existingEvent.Status = EventStatus.Published;
        existingEvent.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Events.Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CancelEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default)
    {
        var existingEvent = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (existingEvent == null)
        {
            return (false, "Event not found");
        }
        if (existingEvent.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        existingEvent.Status = EventStatus.Cancelled;
        existingEvent.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Events.Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DraftEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default)
    {
        var existingEvent = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId && e.OwnerId == ownerId).FirstOrDefaultAsync(cancellationToken);
        if (existingEvent == null)
        {
            return (false, "Event not found");
        }
        if (existingEvent.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        existingEvent.Status = EventStatus.Draft;
        existingEvent.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Events.Update(existingEvent);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private async Task<(bool IsSuccess, string? ErrorMessage)> EnsureOwnerDraftNoConfirmedBookingsAsync(int eventId, int ownerId, CancellationToken cancellationToken)
    {
        var ev = await _unitOfWork.Events.GetByCondition(e => e.Id == eventId).FirstOrDefaultAsync(cancellationToken);
        if (ev == null)
        {
            return (false, "Event not found");
        }
        if (ev.OwnerId != ownerId)
        {
            return (false, "You are not the owner of this event");
        }
        if (ev.Status != EventStatus.Draft)
        {
            return (false, "Event must be in draft status to modify or delete this resource");
        }
        var hasConfirmed = await _unitOfWork.Bookings.GetAll()
            .AnyAsync(b => b.Status == BookingStatus.Confirmed && b.EventSession.EventId == eventId, cancellationToken);
        if (hasConfirmed)
        {
            return (false, "Cannot modify while the event has confirmed bookings");
        }
        return (true, null);
    }

    public async Task<(bool IsSuccess, EventOwnerDashboardDto? Dashboard, string? ErrorMessage)> GetDashboardAsync(int ownerId, EventOwnerDashboardFilterDto? filter = null, CancellationToken cancellationToken = default)
    {
        filter ??= new EventOwnerDashboardFilterDto();
        var pageNumber = filter.EventPerformancePageNumber <= 0 ? 1 : filter.EventPerformancePageNumber;
        var pageSize = filter.EventPerformancePageSize <= 0 ? 10 : Math.Min(filter.EventPerformancePageSize, 50);

        var now = DateTime.UtcNow;
        var events = _unitOfWork.Events.GetAll().AsNoTracking().Where(e => e.OwnerId == ownerId);
        var sessions = _unitOfWork.EventSessions.GetAll().AsNoTracking().Where(s => s.Event.OwnerId == ownerId);
        var seats = _unitOfWork.Seats.GetAll().AsNoTracking().Where(s => s.EventSession.Event.OwnerId == ownerId);
        var ticketTypes = _unitOfWork.TicketTypes.GetAll().AsNoTracking().Where(t => t.EventSession.Event.OwnerId == ownerId);
        var bookings = _unitOfWork.Bookings.GetAll().AsNoTracking().Where(b => b.EventSession.Event.OwnerId == ownerId);
        
        var totalEvents = await events.CountAsync(cancellationToken);
        var draftEvents = await events.Where(e => e.Status == EventStatus.Draft).CountAsync(cancellationToken);
        var publishedEvents = await events.Where(e => e.Status == EventStatus.Published).CountAsync(cancellationToken);
        var cancelledEvents = await events.Where(e => e.Status == EventStatus.Cancelled).CountAsync(cancellationToken);
        var suspendedEvents = await events.Where(e => e.Status == EventStatus.Suspended).CountAsync(cancellationToken);
        var unpublishedEvents = await events
            .Where(e => e.Status != EventStatus.Published && e.Status != EventStatus.Cancelled)
            .CountAsync(cancellationToken);

        var totalSessions = await sessions.CountAsync(cancellationToken);
        var upcomingSessions = await sessions.Where(s => s.StartTime >= now).CountAsync(cancellationToken);
        var pastSessions = await sessions.Where(s => s.StartTime < now).CountAsync(cancellationToken);

        var totalCapacity = (await seats.CountAsync(cancellationToken)) + (await ticketTypes.SumAsync(t => t.TotalQuantity, cancellationToken));
        var soldQuantity = (await seats.Where(s=>s.Status == SeatStatus.Booked).CountAsync()) + (await ticketTypes.SumAsync(t => t.TotalQuantity - t.AvailableQuantity, cancellationToken));
        var availableQuantity = totalCapacity - soldQuantity;

        var totalBookings = await bookings.CountAsync(cancellationToken);
        var confirmedBookings = await bookings.Where(b => b.Status == BookingStatus.Confirmed).CountAsync(cancellationToken);
        var pendingBookings = await bookings.Where(b => b.Status == BookingStatus.Pending).CountAsync(cancellationToken);
        var cancelledBookings = await bookings.Where(b => b.Status == BookingStatus.Cancelled).CountAsync(cancellationToken);
        var totalRevenue = await bookings.SumAsync(b => b.TotalAmount, cancellationToken);

        var eventsPerformanceTotalCount = await events.CountAsync(cancellationToken);
        var eventsPerformanceItems = await events
            .OrderByDescending(e => e.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EventPerformanceDto
            {
                EventId = e.Id,
                EventName = e.Name,
                Status = e.Status,
                BookingMode = e.BookingMode,
                SessionsCount = e.Sessions.Count,
                TotalCapacity = e.Sessions.SelectMany(s => s.Seats).Count()
                    + e.Sessions.SelectMany(s => s.TicketType).Sum(t => (int?)t.TotalQuantity ?? 0),
                SoldQuantity = e.Sessions.SelectMany(s => s.Seats).Count(s => s.Status == SeatStatus.Booked)
                    + e.Sessions.SelectMany(s => s.TicketType).Sum(t => (int?)(t.TotalQuantity - t.AvailableQuantity) ?? 0),
                AvailableQuantity = 0,
                BookingsCount = e.Sessions.SelectMany(s => s.Bookings).Count(),
                Revenue = e.Sessions.SelectMany(s => s.Bookings).Sum(b => b.TotalAmount),
            })
            .ToListAsync(cancellationToken);
        foreach (var row in eventsPerformanceItems)
        {
            row.AvailableQuantity = row.TotalCapacity - row.SoldQuantity;
        }

        var recentBookings = await bookings.OrderByDescending(b => b.CreatedAt).Take(10).Select(b => new RecentBookingDto
        {
            BookingId = b.Id,
            EventName = b.EventSession.Event.Name,
            SessionStartDate = b.EventSession.StartTime,
            Quantity = b.BookingTicketSeats.Count + b.BookingTicketTypes.Count,
            Amount = b.TotalAmount,
            Status = b.Status,
            CreatedAt = b.CreatedAt,
        }).ToListAsync(cancellationToken);

        var dashboard = new EventOwnerDashboardDto
        {
            TotalEvents = totalEvents,
            DraftEvents = draftEvents,
            PublishedEvents = publishedEvents,
            UnpublishedEvents = unpublishedEvents,
            CancelledEvents = cancelledEvents,
            SuspendedEvents = suspendedEvents,
            TotalSessions = totalSessions,
            UpcomingSessions = upcomingSessions,
            PastSessions = pastSessions,
            TotalCapacity = totalCapacity,
            SoldQuantity = soldQuantity,
            AvailableQuantity = availableQuantity,
            TotalBookings = totalBookings,
            ConfirmedBookings = confirmedBookings,
            PendingBookings = pendingBookings,
            CancelledBookings = cancelledBookings,
            TotalRevenue = totalRevenue,
            EventsPerformance = new ApiPagedResponse<EventPerformanceDto>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = eventsPerformanceTotalCount,
                Items = eventsPerformanceItems,
            },
            RecentBookings = recentBookings,
            Alerts = null,
        };
        return (true, dashboard, null);
    }
}
