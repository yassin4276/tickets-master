using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.Booking;

namespace Ticketing.Application.Interfaces.Booking;

public interface IBookingService
{
    Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CreateSeatTypeBookingAsync(CreateSeatTypeBookingDto dto, int userId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CreateTicketTypeBookingAsync(CreateTicketTypeBookingDto dto, int userId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, int BookingId, string? ErrorMessage)> CancelBookingAsync(int bookingId, int userId, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, ApiPagedResponse<BookingDto>? Bookings, string? ErrorMessage)> GetMyBookingsAsync(int userId, MyBookingsFilterDto filter, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, BookingDetailsDto? BookingDetails, string? ErrorMessage)> GetBookingDetailsAsync(int bookingId,int userId, CancellationToken cancellationToken = default);
}
