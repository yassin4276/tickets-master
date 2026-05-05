using System.Buffers.Text;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Ticketing.Application;
using Ticketing.Application.DTOs.Auth;
using Ticketing.Application.Interfaces.Email;
using Ticketing.Infrastructure.Identity;

namespace Ticketing.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IEmailService _emailService;
    private readonly AppUrlSettings _appUrls;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IEmailService emailService,
        AppUrlSettings appUrls,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;
        _appUrls = appUrls;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    private async Task SendEmailConfirmationAsync(
    ApplicationUser user,
    CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            System.Text.Encoding.UTF8.GetBytes(token));

        var baseUrl = _appUrls.PublicBaseUrl.TrimEnd('/');
        var confirmationUrl =
            $"{baseUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        var emailBody = $@"
            <h2>Confirm your email</h2>
            <p>Hello {HtmlEncoder.Default.Encode(user.FullName)},</p>
            <p>Please confirm your email by clicking the link below:</p>
            <p><a href=""{confirmationUrl}"">Confirm Email</a></p>
            <p>If you did not create this account, please ignore this email.</p>
        ";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Confirm your Ticketing account",
            emailBody,
            cancellationToken);
    }

    private async Task SendForgotPasswordEmailAsync(
    ApplicationUser user,
    CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));
        
        var baseUrl = _appUrls.PublicBaseUrl.TrimEnd('/');

        var resetPasswordUrl =
            $"{baseUrl}/api/auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={encodedToken}";

        var emailBody = $@"
            <h2>Reset your password</h2>
            <p>Hello {HtmlEncoder.Default.Encode(user.FullName)},</p>
            <p>You requested to reset your password.</p>
            <p>Use the link below to reset it:</p>
            <p><a href=""{resetPasswordUrl}"">Reset Password</a></p>
            <p>If you did not request this, please ignore this email.</p>
        ";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Reset your Ticketing account password",
            emailBody,
            cancellationToken);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterAsync(RegisterDto registerDto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existingUser != null)
            return (false, "Email is already registered.");

        var ExistingRole = await _roleManager.FindByNameAsync(registerDto.Role);

        if (ExistingRole == null)
        {
            return (false, $"Role '{registerDto.Role}' does not exist.");
        }
        
        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = $"{registerDto.FirstName} {registerDto.LastName}",
            PhoneNumber = registerDto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, $"User creation failed: {errors}");
        }
        
        var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);
        if (!roleResult.Succeeded){
            var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
            return (false, $"Assigning role failed: {errors}");
        }

        await SendEmailConfirmationAsync(user, cancellationToken);

        return (true, null);

    }

    public async Task<(bool IsSuccess, AuthTokensDto? Tokens, string? ErrorMessage)> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
            return (false, null, "Invalid email or password.");
        
        var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!passwordValid)
            return (false, null, "Invalid email or password.");
        
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            await SendEmailConfirmationAsync(user, cancellationToken);
            return (false, null, "Please verify your email. A new confirmation email has been sent.");
        }

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
        var email = user.Email ?? loginDto.Email;
        var access = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.FullName, email, role);
        var refresh = _jwtTokenGenerator.GenerateRefreshToken(user.Id.ToString(), user.FullName, email, role);

        return (true, new AuthTokensDto
        {
            AccessToken = access.Token,
            RefreshToken = refresh.Token,
            AccessTokenExpiresAt = access.ExpiresAt,
            RefreshTokenExpiresAt = refresh.ExpiresAt
        }, null);
    }

    public async Task<(bool IsSuccess, AuthTokensDto? Tokens, string? ErrorMessage)> RefreshTokenAsync(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!_jwtTokenGenerator.ValidateRefreshToken(request.RefreshToken, out var userId, out _, out _, out _))
            return (false, null, "Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return (false, null, "User no longer exists.");

        if (!await _userManager.IsEmailConfirmedAsync(user))
            return (false, null, "Email is not confirmed.");

        var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
        var email = user.Email ?? string.Empty;
        var access = _jwtTokenGenerator.GenerateToken(user.Id.ToString(), user.FullName, email, role);
        var refresh = _jwtTokenGenerator.GenerateRefreshToken(user.Id.ToString(), user.FullName, email, role);

        return (true, new AuthTokensDto
        {
            AccessToken = access.Token,
            RefreshToken = refresh.Token,
            AccessTokenExpiresAt = access.ExpiresAt,
            RefreshTokenExpiresAt = refresh.ExpiresAt
        }, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
        if (user == null)
            return (false, "User not found.");

        if (user.EmailConfirmed)
        {
            return (true, null);
        }

        var decodedTokenBytes = WebEncoders.Base64UrlDecode(confirmEmailDto.Token);
        var decodedToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ConfirmEmailFromLinkAsync(
        int userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return (false, "User not found.");

        if (user.EmailConfirmed)
            return (true, null);

        var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
        var decodedToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ResendConfirmationEmailAsync(string email,CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return (false, "User not found.");
        }

        if (user.EmailConfirmed)
        {
            return (false, "Email is already confirmed.");
        }

        await SendEmailConfirmationAsync(user, cancellationToken);

        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto,CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

        if (user is null)
        {
            // Security-wise: متقولش للمستخدم إن الإيميل مش موجود
            return (true, null);
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            await SendEmailConfirmationAsync(user, cancellationToken);

            return (false, "Please verify your email first. A confirmation email has been sent.");
        }

        await SendForgotPasswordEmailAsync(user, cancellationToken);

        return (true, null);
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto,CancellationToken cancellationToken = default)
    {
        if (resetPasswordDto.NewPassword != resetPasswordDto.ConfirmNewPassword)
        {
            return (false, "New password and confirmation password do not match.");
        }

        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);

        if (user is null)
        {
            return (false, "Invalid reset password request.");
        }

        var decodedTokenBytes = WebEncoders.Base64UrlDecode(resetPasswordDto.Token);
        var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

        var result = await _userManager.ResetPasswordAsync(
            user,
            decodedToken,
            resetPasswordDto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return (false, errors);
        }

        return (true, null);
    }
}
