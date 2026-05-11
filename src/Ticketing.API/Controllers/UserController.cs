using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.Booking;
using Ticketing.Application.DTOs.User;
using Ticketing.Application.Interfaces.Booking;
using Ticketing.Application.Interfaces.User;

namespace Ticketing.API.Controllers;

[Route("api/user")]
[ApiController]
[Authorize(Roles = "User")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBookingService _bookingService;

    public UserController(IUserService userService, IBookingService bookingService)
    {
        _userService = userService;
        _bookingService = bookingService;
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return int.TryParse(userIdValue, out userId);
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents([FromQuery] UserEventsFilterDto filter, CancellationToken cancellationToken)
    {
        var result = await _userService.GetEventsAsync(filter, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<ApiPagedResponse<EventDto>>.Ok(result.Events!, "Events fetched successfully"));
    }

    [HttpGet("events/{eventId:int}")]
    public async Task<IActionResult> GetEventById(int eventId, CancellationToken cancellationToken)
    {
        var result = await _userService.GetEventByIdAsync(eventId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<EventDto>.Ok(result.Event!, "Event fetched successfully"));
    }

    [HttpGet("events/{eventId:int}/sessions")]
    public async Task<IActionResult> GetSessions(int eventId, CancellationToken cancellationToken)
    {
        var result = await _userService.GetSessionsAsync(eventId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<List<SessionsDto>>.Ok(result.Sessions!, "Sessions fetched successfully"));
    }

    [HttpGet("sessions/{sessionId:int}/seats")]
    public async Task<IActionResult> GetSeats(int sessionId, CancellationToken cancellationToken)
    {
        var result = await _userService.GetSeatsAsync(sessionId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<List<SeatsDto>>.Ok(result.Seats!, "Seats fetched successfully"));
    }

    [HttpGet("sessions/{sessionId:int}/ticket-types")]
    public async Task<IActionResult> GetTicketTypes(int sessionId, CancellationToken cancellationToken)
    {
        var result = await _userService.GetTicketTypesAsync(sessionId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<List<TicketTypeDto>>.Ok(result.TicketTypes!, "Ticket types fetched successfully"));
    }

    [HttpPost("bookings/seat-type")]
    public async Task<IActionResult> CreateSeatTypeBooking(CreateSeatTypeBookingDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _bookingService.CreateSeatTypeBookingAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<int>.Ok(result.BookingId, "Booking created successfully"));
    }

    [HttpPost("bookings/ticket-type")]
    public async Task<IActionResult> CreateTicketTypeBooking(CreateTicketTypeBookingDto dto, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _bookingService.CreateTicketTypeBookingAsync(dto, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<int>.Ok(result.BookingId, "Booking created successfully"));
    }

    [HttpPut("bookings/{bookingId:int}")]
    public async Task<IActionResult> CancelBooking(int bookingId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _bookingService.CancelBookingAsync(bookingId, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<string?>.Ok(null, "Booking cancelled successfully"));
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetMyBookings([FromQuery] MyBookingsFilterDto filter, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _bookingService.GetMyBookingsAsync(userId, filter, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<ApiPagedResponse<BookingDto>>.Ok(result.Bookings!, "Bookings fetched successfully"));
    }

    [HttpGet("bookings/{bookingId:int}")]
    public async Task<IActionResult> GetBookingDetails(int bookingId, CancellationToken cancellationToken)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(ApiResponse<string>.Fail("Invalid user token."));
        }

        var result = await _bookingService.GetBookingDetailsAsync(bookingId, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
        }

        return Ok(ApiResponse<BookingDetailsDto>.Ok(result.BookingDetails!, "Booking details fetched successfully"));
    }
}
