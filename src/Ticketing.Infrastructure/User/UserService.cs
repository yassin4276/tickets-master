using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Responses;
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

    public async Task<(bool IsSuccess, ApiPagedResponse<EventDto>? Events, string? ErrorMessage)> GetEventsAsync(UserEventsFilterDto filter, CancellationToken cancellationToken = default)
    {
        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
        pageSize = Math.Min(pageSize, 50);

        var query = _unitOfWork.Events.GetAll().AsNoTracking()
            .Where(e => e.Status == EventStatus.Published);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(e => e.Name.Contains(term) || e.Description.Contains(term));
        }

        if (filter.Id.HasValue)
        {
            query = query.Where(e => e.Id == filter.Id.Value);
        }

        if (filter.OwnerId.HasValue)
        {
            query = query.Where(e => e.OwnerId == filter.OwnerId.Value);
        }

        if (filter.BookingMode.HasValue)
        {
            query = query.Where(e => e.BookingMode == filter.BookingMode.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            var name = filter.Name.Trim();
            query = query.Where(e => e.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(filter.Description))
        {
            var desc = filter.Description.Trim();
            query = query.Where(e => e.Description.Contains(desc));
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
                BookingMode = e.BookingMode,
                OwnerId = e.OwnerId,
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
