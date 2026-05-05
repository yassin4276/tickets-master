using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.EventOwner;

namespace Ticketing.Application.Interfaces.EventOwner;

public interface IEventOwnerService
{
    Task<(bool IsSuccess, EventDto? Data, string? ErrorMessage)> CreateEventAsync(int ownerId, CreateEventDto createEventDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateEventAsync(int eventId, UpdateEventDto updateEventDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, EventDto? Event, string? ErrorMessage)> GetEventByIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, ApiPagedResponse<EventDto>? Events, string? ErrorMessage)> GetMyEventsAsync(int ownerId, EventOwnerEventsFilterDto filterDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> CreateEventSessionAsync(int eventId, int ownerId, CreateEventSessionDto createEventSessionDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateEventSessionAsync(int sessionId, int ownerId, UpdateEventSessionDto dto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> DeleteEventSessionAsync(int sessionId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> CreateSeatAsync(int eventSessionId, int ownerId, CreatSeatDto createSeatDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateSeatAsync(int seatId, int ownerId, UpdateSeatDto dto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> DeleteSeatAsync(int seatId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> CreateTicketTypeAsync(int eventSessionId, int ownerId, CreateTicketTypeDto createTicketTypeDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> UpdateTicketTypeAsync(int ticketTypeId, int ownerId, UpdateTicketTypeDto dto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> DeleteTicketTypeAsync(int ticketTypeId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> PublishEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> CancelEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> DraftEventAsync(int eventId, int ownerId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, EventOwnerDashboardDto? Dashboard, string? ErrorMessage)> GetDashboardAsync(int ownerId, EventOwnerDashboardFilterDto? filter = null, CancellationToken cancellationToken = default);
}
