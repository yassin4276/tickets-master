using Microsoft.EntityFrameworkCore;
using Ticketing.Application.DTOs.User;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.User;
using Ticketing.Domain.Enums;

namespace Ticketing.Infrastructure.User;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool IsSuccess, EventDto? Event, string? ErrorMessage)> GetEventByIdAsync(int eventId,CancellationToken cancellationToken = default)
    {
        var eventDto = await _unitOfWork.Events
            .GetAll()
            .AsNoTracking()
            .Where(e => e.Id == eventId && e.Status == EventStatus.Published)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                BookingMode = e.BookingMode,
                OwnerId = e.OwnerId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (eventDto == null)
        {
            return (false, null, "Event not found");
        }

        return (true, eventDto, null);
    }

    public async Task<(bool IsSuccess, List<EventDto> Events, string? ErrorMessage)> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = await _unitOfWork.Events.GetAll().AsNoTracking().Where(e=>e.Status == EventStatus.Published)
        .Select(e => new EventDto {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            BookingMode = e.BookingMode,
            OwnerId = e.OwnerId,
        }).ToListAsync(cancellationToken);
        return (true, events, null);
    }


    public async Task<(bool IsSuccess, List<SessionsDto> Sessions, string? ErrorMessage)> GetSessionsAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var sessions = await _unitOfWork.EventSessions.GetAll()
        .AsNoTracking()
        .Where(e=>e.EventId == eventId && e.Status == SessionStatus.Scheduled && e.Event.Status == EventStatus.Published && e.StartTime > DateTime.UtcNow)
        .OrderBy(e=>e.StartTime)
        .Select(e=>new SessionsDto{
            Id = e.Id,
            Day = DateOnly.FromDateTime(e.StartTime),
            StartTime = TimeOnly.FromDateTime(e.StartTime),
            EndTime = TimeOnly.FromDateTime(e.EndTime),
            Location = e.Location,
            Status = e.Status,
        }).ToListAsync(cancellationToken);
        return (true, sessions, null);
    }

    public async Task<(bool IsSuccess, List<SeatsDto> Seats, string? ErrorMessage)> GetSeatsAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var seats = await _unitOfWork.Seats.GetAll()
        .AsNoTracking()
        .Where(e=>e.EventSessionId == sessionId 
               && e.EventSession.Status == SessionStatus.Scheduled 
               && e.EventSession.Event.Status == EventStatus.Published 
               && e.EventSession.Event.BookingMode == EventBookingMode.Seats)
        .Select(e=>new SeatsDto{
            Id = e.Id,
            SeatNumber = e.SeatNumber,
            Type = e.Type,
            Status = e.Status,
            Price = e.Price,
        }).ToListAsync(cancellationToken);
        return (true, seats, null);
    }
    public async Task<(bool IsSuccess, List<TicketTypeDto> TicketTypes, string? ErrorMessage)> GetTicketTypesAsync(int sessionId, CancellationToken cancellationToken = default)
    {
        var ticketTypes = await _unitOfWork.TicketTypes.GetAll()
        .AsNoTracking()
        .Where(e=>e.EventSessionId == sessionId 
               && e.Status == TicketTypeStatus.Active 
               && e.EventSession.Status == SessionStatus.Scheduled 
               && e.EventSession.Event.Status == EventStatus.Published
               && e.EventSession.Event.BookingMode == EventBookingMode.Tickets)
        .Select(e=>new TicketTypeDto{
            Id = e.Id,
            Name = e.Name,
            Price = e.Price,
            AvailableQuantity = e.AvailableQuantity,
            Status = e.Status,
        }).ToListAsync(cancellationToken);
        return (true, ticketTypes, null);
    }
}
