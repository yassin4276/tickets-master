using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.EventOwner;
using Ticketing.Application.Interfaces.EventOwner;

namespace Ticketing.API.Controllers;

[Route("api/event-owner")]
[ApiController]
public class EventOwnerController : ControllerBase
{
    private readonly IEventOwnerService _eventOwnerService;

    public EventOwnerController(IEventOwnerService eventOwnerService)
    {
        _eventOwnerService = eventOwnerService;
    }

    private bool TryGetOwnerId(out int ownerId)
    {
        var userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(userIdValue, out ownerId);
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent(CreateEventDto createEventDto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _eventOwnerService.CreateEventAsync(ownerId, createEventDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<EventDto>.Ok(result.Data!, "Event created successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpGet("events")]
    public async Task<IActionResult> GetMyEvents([FromQuery] EventOwnerEventsFilterDto filterDto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.GetMyEventsAsync(ownerId, filterDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<ApiPagedResponse<EventDto>>.Ok(result.Events!, "Events fetched successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpGet("events/{eventId:int}")]
    public async Task<IActionResult> GetEventById(int eventId)
    {
        var result = await _eventOwnerService.GetEventByIdAsync(eventId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<EventDto>.Ok(result.Event!, "Event fetched successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPut("events/{eventId:int}")]
    public async Task<IActionResult> UpdateEvent(int eventId, UpdateEventDto updateEventDto)
    {
        var result = await _eventOwnerService.UpdateEventAsync(eventId, updateEventDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event updated successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("events/{eventId:int}/sessions")]
    public async Task<IActionResult> CreateEventSession(int eventId, CreateEventSessionDto createEventSessionDto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.CreateEventSessionAsync(eventId, ownerId, createEventSessionDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event session created successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPut("sessions/{sessionId:int}")]
    public async Task<IActionResult> UpdateEventSession(int sessionId, UpdateEventSessionDto dto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.UpdateEventSessionAsync(sessionId, ownerId, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event session updated successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpDelete("sessions/{sessionId:int}")]
    public async Task<IActionResult> DeleteEventSession(int sessionId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.DeleteEventSessionAsync(sessionId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event session deleted successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("sessions/{sessionId:int}/seats")]
    public async Task<IActionResult> CreateSeat(int sessionId, CreatSeatDto createSeatDto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.CreateSeatAsync(sessionId, ownerId, createSeatDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Seat created successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPut("seats/{seatId:int}")]
    public async Task<IActionResult> UpdateSeat(int seatId, UpdateSeatDto dto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.UpdateSeatAsync(seatId, ownerId, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Seat updated successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpDelete("seats/{seatId:int}")]
    public async Task<IActionResult> DeleteSeat(int seatId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.DeleteSeatAsync(seatId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Seat deleted successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("sessions/{sessionId:int}/ticket-types")]
    public async Task<IActionResult> CreateTicketType(int sessionId, CreateTicketTypeDto createTicketTypeDto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.CreateTicketTypeAsync(sessionId, ownerId, createTicketTypeDto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Ticket type created successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPut("ticket-types/{ticketTypeId:int}")]
    public async Task<IActionResult> UpdateTicketType(int ticketTypeId, UpdateTicketTypeDto dto)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.UpdateTicketTypeAsync(ticketTypeId, ownerId, dto);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Ticket type updated successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpDelete("ticket-types/{ticketTypeId:int}")]
    public async Task<IActionResult> DeleteTicketType(int ticketTypeId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.DeleteTicketTypeAsync(ticketTypeId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Ticket type deleted successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("events/{eventId:int}/publish")]
    public async Task<IActionResult> PublishEvent(int eventId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.PublishEventAsync(eventId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event published successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("events/{eventId:int}/cancel")]
    public async Task<IActionResult> CancelEvent(int eventId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.CancelEventAsync(eventId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event cancelled successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpPost("events/{eventId:int}/draft")]
    public async Task<IActionResult> DraftEvent(int eventId)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.DraftEventAsync(eventId, ownerId);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<string?>.Ok(null, "Event drafted successfully"));
    }

    [Authorize(Roles = "EventOwner")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] EventOwnerDashboardFilterDto filter)
    {
        if (!TryGetOwnerId(out var ownerId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }
        var result = await _eventOwnerService.GetDashboardAsync(ownerId, filter);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }
        return Ok(ApiResponse<EventOwnerDashboardDto>.Ok(result.Dashboard!, "Dashboard fetched successfully"));
    }
}