using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Application;
using Ticketing.Application.Common.Responses;
using Ticketing.Application.DTOs.Auth;

namespace Ticketing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
            }
            return Ok(ApiResponse<string?>.Ok(null, "User registered successfully"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
            }
            return Ok(ApiResponse<AuthTokensDto>.Ok(result.Tokens!, "User logged in successfully"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
            }
            return Ok(ApiResponse<AuthTokensDto>.Ok(result.Tokens!, "Token refreshed successfully"));
        }
        
        /// <summary>Used when the user opens the confirmation link from email (GET + query string).</summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailFromLink([FromQuery] int userId, [FromQuery] string token)
        {
            var result = await _authService.ConfirmEmailFromLinkAsync(userId, token);
            if (!result.IsSuccess)
            {
                var msg = WebUtility.HtmlEncode(result.ErrorMessage ?? "Unknown error");
                return Content(
                    $"<html><body><p>Could not confirm your email: {msg}</p></body></html>",
                    "text/html");
            }

            return Content(
                "<html><body><p>Your email is confirmed. You can close this page and sign in.</p></body></html>",
                "text/html");
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            var result = await _authService.ConfirmEmailAsync(confirmEmailDto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
            }
            return Ok(ApiResponse<string?>.Ok(null, "Email confirmed successfully"));
        }

        [HttpPost("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail(string email)
        {
            var result = await _authService.ResendConfirmationEmailAsync(email);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong"}));
            }
            return Ok(ApiResponse<string?>.Ok(null, "Confirmation email sent successfully"));
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong"}));
            }
            return Ok(ApiResponse<string?>.Ok(null, "Password reset email sent successfully"));
        }

        /// <summary>Opens the reset page from the email link (GET). User submits the form to complete reset.</summary>
        [HttpGet("reset-password")]
        public IActionResult ResetPasswordPage([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return Content(
                    "<html><body><p>This reset link is invalid or incomplete.</p></body></html>",
                    "text/html");
            }

            var e = WebUtility.HtmlEncode(email);
            var t = WebUtility.HtmlEncode(token);

            return Content(
                $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""/><title>Reset password</title></head>
<body>
<h2>Reset your password</h2>
<form method=""post"" action=""/api/auth/reset-password"" enctype=""application/x-www-form-urlencoded"">
  <input type=""hidden"" name=""Email"" value=""{e}"" />
  <input type=""hidden"" name=""Token"" value=""{t}"" />
  <p><label>New password<br/><input type=""password"" name=""NewPassword"" required minlength=""6"" autocomplete=""new-password""/></label></p>
  <p><label>Confirm new password<br/><input type=""password"" name=""ConfirmNewPassword"" required minlength=""6"" autocomplete=""new-password""/></label></p>
  <button type=""submit"">Save new password</button>
</form>
</body></html>",
                "text/html");
        }

        /// <summary>Browser form POST from the email reset page. Hidden from Swagger (same path as JSON reset would conflict in OpenAPI).</summary>
        [HttpPost("reset-password")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> ResetPasswordFromForm([FromForm] ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.IsSuccess)
            {
                var msg = WebUtility.HtmlEncode(result.ErrorMessage ?? "Unknown error");
                return Content(
                    $"<html><body><p>Could not reset password: {msg}</p><p><a href=\"javascript:history.back()\">Try again</a></p></body></html>",
                    "text/html");
            }

            return Content(
                "<html><body><p>Your password was updated. You can close this page and sign in.</p></body></html>",
                "text/html");
        }

        [HttpPost("reset-password")]
        [Consumes("application/json")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var result = await _authService.ResetPasswordAsync(resetPasswordDto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<string>.Fail(new List<string> { result.ErrorMessage ?? "Something went wrong" }));
            }
            return Ok(ApiResponse<string?>.Ok(null, "Password reset successfully"));
        }

    }
}
