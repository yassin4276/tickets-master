using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.Booking;
using Ticketing.Application.Interfaces.Booking;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;
using Ticketing.Infrastructure.RealTime;

namespace Ticketing.Infrastructure.Booking;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<BookingNotificationHub> _hubContext;

    public BookingService(IUnitOfWork unitOfWork, IHubContext<BookingNotificationHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }
    private string GenerateBookingNumber()
    {
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var randomPart = Guid.NewGuid().ToString("N")[..8].ToUpper();

        return $"TM-{datePart}-{randomPart}";
    }

    private async Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> FailBookingAsync(string message,CancellationToken cancellationToken = default)
    {
        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
        return (false, 0, message);
    }

    public async Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CreateSeatTypeBookingAsync(CreateSeatTypeBookingDto dto, int userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _unitOfWork.EventSessions
            .GetAll()
            .Include(s => s.Event)
            .FirstOrDefaultAsync(s => s.Id == dto.SessionId, cancellationToken);

            if (session == null)
                return await FailBookingAsync("Session not found", cancellationToken);

            if (session.Event.BookingMode != EventBookingMode.Seats)
                return await FailBookingAsync("Event booking mode is not seats", cancellationToken);

            if (session.Event.Status != EventStatus.Published)
                return await FailBookingAsync("Event is not published", cancellationToken);

            if (session.Status != SessionStatus.Scheduled)
                return await FailBookingAsync("Session is not active", cancellationToken);

            if (session.StartTime <= DateTime.UtcNow)
                return await FailBookingAsync("Cannot book a session that already started", cancellationToken);

            if (dto.SeatIds == null || dto.SeatIds.Count == 0)
                return await FailBookingAsync("No seats selected", cancellationToken);

            var seatIds = dto.SeatIds.Distinct().ToList();

            if (seatIds.Count != dto.SeatIds.Count)
                return await FailBookingAsync("Duplicate seats are not allowed", cancellationToken);

            var seats = await _unitOfWork.Seats
                .GetAll()
                .Where(s => seatIds.Contains(s.Id) && s.EventSessionId == dto.SessionId)
                .ToListAsync(cancellationToken);

            if (seats.Count != seatIds.Count)
                return await FailBookingAsync("One or more seats were not found in this session", cancellationToken);

            if (seats.Any(s => s.Status != SeatStatus.Available))
                return await FailBookingAsync("One or more seats are not available", cancellationToken);

            var totalPrice = seats.Sum(s => s.Price);

            var booking = new Domain.Entities.Booking
            {
                Status = BookingStatus.Confirmed,
                BookingNumber = GenerateBookingNumber(),
                TotalAmount = totalPrice,
                EventSessionId = dto.SessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);

            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Booked;

                var bookingSeat = new BookingTicketSeat
                {
                    Price = seat.Price,
                    Booking = booking,
                    SeatId = seat.Id,
                };

                await _unitOfWork.BookingSeats.AddAsync(bookingSeat, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await NotifySeatsBookedAsync(dto.SessionId, seats.Select(s => s.Id).ToList(), cancellationToken);

            return (true, booking.Id, null);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            return (false, 0, "Booking failed. Please try again.");
        }
        
    }

    public async Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CreateTicketTypeBookingAsync(CreateTicketTypeBookingDto dto, int userId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var session = await _unitOfWork.EventSessions
            .GetAll()
            .Include(s => s.Event)
            .FirstOrDefaultAsync(s => s.Id == dto.SessionId, cancellationToken);

            if (session == null)
                return await FailBookingAsync("Session not found", cancellationToken);

            if (session.Event.BookingMode != EventBookingMode.Tickets)
                return await FailBookingAsync("Event booking mode is not tickets", cancellationToken);

            if (session.Event.Status != EventStatus.Published)
                return await FailBookingAsync("Event is not published", cancellationToken);

            if (session.Status != SessionStatus.Scheduled)
                return await FailBookingAsync("Session is not active", cancellationToken);

            if (session.StartTime <= DateTime.UtcNow)
                return await FailBookingAsync("Cannot book a session that already started", cancellationToken);

            if (dto.TicketTypes == null || dto.TicketTypes.Count == 0)
                return await FailBookingAsync("No ticket types selected", cancellationToken);

            if (dto.TicketTypes.Any(item => item.Quantity <= 0))
                return await FailBookingAsync("Quantity must be greater than zero", cancellationToken);

            var ticketTypeIds = dto.TicketTypes
                .Select(item => item.TicketTypeId)
                .ToList();

            if (ticketTypeIds.Distinct().Count() != ticketTypeIds.Count)
                return await FailBookingAsync("Duplicate ticket types are not allowed", cancellationToken);

            var ticketTypes = await _unitOfWork.TicketTypes
                .GetAll()
                .Where(ticketType =>
                    ticketTypeIds.Contains(ticketType.Id) &&
                    ticketType.EventSessionId == dto.SessionId)
                .ToListAsync(cancellationToken);

            if (ticketTypes.Count != ticketTypeIds.Count)
                return await FailBookingAsync("One or more ticket types were not found in this session", cancellationToken);

            if (ticketTypes.Any(ticketType => ticketType.Status != TicketTypeStatus.Active))
                return await FailBookingAsync("One or more ticket types are not active", cancellationToken);

            foreach (var item in dto.TicketTypes)
            {
                var ticketType = ticketTypes.First(ticketType => ticketType.Id == item.TicketTypeId);

                if (ticketType.AvailableQuantity < item.Quantity)
                    return await FailBookingAsync($"Not enough available quantity for {ticketType.Name}", cancellationToken);
            }

            var totalPrice = dto.TicketTypes.Sum(item =>
            {
                var ticketType = ticketTypes.First(ticketType => ticketType.Id == item.TicketTypeId);
                return ticketType.Price * item.Quantity;
            });

            var booking = new Domain.Entities.Booking
            {
                Status = BookingStatus.Confirmed,
                BookingNumber = GenerateBookingNumber(),
                TotalAmount = totalPrice,
                EventSessionId = dto.SessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.Bookings.AddAsync(booking, cancellationToken);

            foreach (var item in dto.TicketTypes)
            {
                var ticketType = ticketTypes.First(ticketType => ticketType.Id == item.TicketTypeId);

                ticketType.AvailableQuantity -= item.Quantity;

                var bookingTicketType = new BookingTicketType
                {
                    Quantity = item.Quantity,
                    UnitPrice = ticketType.Price,
                    Booking = booking,
                    TicketTypes = ticketType,
                };

                await _unitOfWork.BookingTicketTypes.AddAsync(bookingTicketType, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await NotifyTicketAvailabilityAsync(
                dto.SessionId,
                ticketTypes.Select(t => (t.Id, t.AvailableQuantity)).ToList(),
                cancellationToken);

            return (true, booking.Id, null);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return (false, 0, "Booking failed. Please try again.");
        }
        
    }

    public async Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CancelBookingAsync(int bookingId,int userId,CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var booking = await _unitOfWork.Bookings
                .GetAll()
                .Include(b => b.EventSession)
                    .ThenInclude(es => es.Event)
                .Include(b => b.BookingTicketSeats)
                    .ThenInclude(bs => bs.Seat)
                .Include(b => b.BookingTicketTypes)
                    .ThenInclude(bt => bt.TicketTypes)
            .FirstOrDefaultAsync(
                b => b.Id == bookingId && b.UserId == userId,
                cancellationToken);

        if (booking == null)
            return await FailBookingAsync("Booking not found" , cancellationToken);

        if (booking.Status == BookingStatus.Cancelled)
            return await FailBookingAsync("Booking is already cancelled", cancellationToken);

        if (booking.EventSession.StartTime <= DateTime.UtcNow)
            return await FailBookingAsync("Cannot cancel booking after session has started", cancellationToken);

        if (booking.Status != BookingStatus.Confirmed && booking.Status != BookingStatus.Pending)
            return await FailBookingAsync("Booking cannot be cancelled", cancellationToken);

        foreach (var bookingSeat in booking.BookingTicketSeats)
        {
            bookingSeat.Seat.Status = SeatStatus.Available;
        }

        foreach (var bookingTicketType in booking.BookingTicketTypes)
        {
            bookingTicketType.TicketTypes.AvailableQuantity += bookingTicketType.Quantity;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        await NotifyBookingCancelledAsync(booking, cancellationToken);

        return (true, booking.Id, null);
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return (false, 0, "Booking failed. Please try again.");
        }
        
    }

    private Task NotifySeatsBookedAsync(int sessionId, List<int> seatIds, CancellationToken cancellationToken)
    {
        if (seatIds.Count == 0)
            return Task.CompletedTask;

        var message = new SeatBookingRealtimeMessage
        {
            SessionId = sessionId,
            SeatIds = seatIds,
            Status = SeatStatus.Booked.ToString()
        };

        return _hubContext.Clients
            .Group(BookingNotificationHub.SessionGroupName(sessionId))
            .SendAsync("ReceiveSeatBookingUpdate", message, cancellationToken);
    }

    private Task NotifySeatsReleasedAsync(int sessionId, List<int> seatIds, CancellationToken cancellationToken)
    {
        if (seatIds.Count == 0)
            return Task.CompletedTask;

        var message = new SeatBookingRealtimeMessage
        {
            SessionId = sessionId,
            SeatIds = seatIds,
            Status = SeatStatus.Available.ToString()
        };

        return _hubContext.Clients
            .Group(BookingNotificationHub.SessionGroupName(sessionId))
            .SendAsync("ReceiveSeatBookingUpdate", message, cancellationToken);
    }

    private Task NotifyTicketAvailabilityAsync(
        int sessionId,
        IReadOnlyList<(int TicketTypeId, int AvailableQuantity)> updates,
        CancellationToken cancellationToken)
    {
        if (updates.Count == 0)
            return Task.CompletedTask;

        var message = new TicketAvailabilityRealtimeMessage
        {
            SessionId = sessionId,
            Updates = updates
                .Select(u => new TicketTypeAvailabilityItem
                {
                    TicketTypeId = u.TicketTypeId,
                    AvailableQuantity = u.AvailableQuantity
                })
                .ToList()
        };

        return _hubContext.Clients
            .Group(BookingNotificationHub.SessionGroupName(sessionId))
            .SendAsync("ReceiveTicketAvailabilityUpdate", message, cancellationToken);
    }

    private async Task NotifyBookingCancelledAsync(global::Ticketing.Domain.Entities.Booking booking, CancellationToken cancellationToken)
    {
        var sessionId = booking.EventSessionId;

        if (booking.EventSession.Event.BookingMode == EventBookingMode.Seats)
        {
            var seatIds = booking.BookingTicketSeats.Select(bs => bs.SeatId).ToList();
            await NotifySeatsReleasedAsync(sessionId, seatIds, cancellationToken);
            return;
        }

        if (booking.EventSession.Event.BookingMode == EventBookingMode.Tickets)
        {
            var updates = booking.BookingTicketTypes
                .Select(bt => (bt.TicketTypesId, bt.TicketTypes.AvailableQuantity))
                .ToList();
            await NotifyTicketAvailabilityAsync(sessionId, updates, cancellationToken);
        }
    }

    public async Task<(bool IsSuccess, ApiPagedResponse<BookingDto>? Bookings, string? ErrorMessage)> GetMyBookingsAsync(int userId, MyBookingsFilterDto filter, CancellationToken cancellationToken = default)
    {
        var pageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;
        pageSize = Math.Min(pageSize, 50);

        var query = _unitOfWork.Bookings
            .GetAll()
            .AsNoTracking()
            .Where(b => b.UserId == userId);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(b =>
                b.BookingNumber.Contains(term)
                || b.EventSession.Event.Name.Contains(term)
                || b.EventSession.Location.Contains(term));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(b => b.Status == filter.Status.Value);
        }
        if (filter.MinTotalAmount.HasValue)
        {
            query = query.Where(b => b.TotalAmount >= filter.MinTotalAmount.Value);
        }

        if (filter.MaxTotalAmount.HasValue)
        {
            query = query.Where(b => b.TotalAmount <= filter.MaxTotalAmount.Value);
        }

        if (filter.SessionStartFrom.HasValue)
        {
            query = query.Where(b => b.EventSession.StartTime >= filter.SessionStartFrom.Value);
        }

        if (filter.SessionStartTo.HasValue)
        {
            query = query.Where(b => b.EventSession.StartTime <= filter.SessionStartTo.Value);
        }

        

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(b => b.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                EventName = b.EventSession.Event.Name,
                EventSessionId = b.EventSessionId,
                SessionStartTime = b.EventSession.StartTime,
                SessionEndTime = b.EventSession.EndTime,
                Location = b.EventSession.Location,
                CreatedAt = b.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var response = new ApiPagedResponse<BookingDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items
        };

        return (true, response, null);
    }

    public async Task<(bool IsSuccess, BookingDetailsDto? BookingDetails, string? ErrorMessage)> GetBookingDetailsAsync(int bookingId,int userId, CancellationToken cancellationToken = default)
    {
        var booking = await _unitOfWork.Bookings
        .GetAll()
        .AsNoTracking()
        .Include(b => b.EventSession)
            .ThenInclude(es => es.Event)
        .FirstOrDefaultAsync(
            b => b.Id == bookingId && b.UserId == userId,
            cancellationToken);

        if (booking == null)
            return (false, null, "Booking not found");

        var bookingDetails = new BookingDetailsDto
        {
            Id = booking.Id,
            BookingNumber = booking.BookingNumber,
            TotalAmount = booking.TotalAmount,
            Status = booking.Status,
            EventName = booking.EventSession.Event.Name,
            EventSessionId = booking.EventSessionId,
            SessionStartTime = booking.EventSession.StartTime,
            SessionEndTime = booking.EventSession.EndTime,
            Location = booking.EventSession.Location,
            CreatedAt = booking.CreatedAt,
            Seats = new List<BookingSeatDetailsDto>(),
            TicketTypes = new List<BookingTicketTypeDetailsDto>()
        };

        if (booking.EventSession.Event.BookingMode == EventBookingMode.Seats)
        {
            bookingDetails.Seats = await _unitOfWork.BookingSeats
                .GetAll()
                .AsNoTracking()
                .Where(bs => bs.BookingId == bookingId)
                .Select(bs => new BookingSeatDetailsDto
                {
                    SeatId = bs.SeatId,
                    SeatNumber = bs.Seat.SeatNumber,
                    Type = bs.Seat.Type,
                    Price = bs.Price,
                })
                .ToListAsync(cancellationToken);

            return (true, bookingDetails, null);
        }

        if (booking.EventSession.Event.BookingMode == EventBookingMode.Tickets)
        {
            bookingDetails.TicketTypes = await _unitOfWork.BookingTicketTypes
                .GetAll()
                .AsNoTracking()
                .Where(bt => bt.BookingId == bookingId)
                .Select(bt => new BookingTicketTypeDetailsDto
                {
                    TicketTypeId = bt.TicketTypesId,
                    Name = bt.TicketTypes.Name,
                    Quantity = bt.Quantity,
                    UnitPrice = bt.UnitPrice,
                    TotalPrice = bt.Quantity * bt.UnitPrice,
                })
                .ToListAsync(cancellationToken);

            return (true, bookingDetails, null);
        }

        return (false, null, "Booking mode not supported");
    }
}
