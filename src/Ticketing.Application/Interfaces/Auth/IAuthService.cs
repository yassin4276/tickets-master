using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application;

public interface IAuthService
{
    Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, AuthTokensDto? Tokens, string? ErrorMessage)> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, AuthTokensDto? Tokens, string? ErrorMessage)> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto,CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> ConfirmEmailFromLinkAsync(int userId, string token, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> ResendConfirmationEmailAsync(string email,CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto,CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto,CancellationToken cancellationToken = default);
}
