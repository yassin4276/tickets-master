using Ticketing.Application.DTOs.User;

namespace Ticketing.Application.Interfaces.User;

public interface IUserService
{
    Task<(bool IsSuccess, List<EventDto> Events, string? ErrorMessage)> GetEventsAsync(CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, EventDto? Event, string? ErrorMessage)> GetEventByIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, List<SessionsDto> Sessions, string? ErrorMessage)> GetSessionsAsync(int eventId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, List<SeatsDto> Seats, string? ErrorMessage)> GetSeatsAsync(int sessionId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, List<TicketTypeDto> TicketTypes, string? ErrorMessage)> GetTicketTypesAsync(int sessionId, CancellationToken cancellationToken = default);
}
