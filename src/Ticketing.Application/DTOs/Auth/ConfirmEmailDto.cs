using System.ComponentModel.DataAnnotations;

namespace Ticketing.Application.DTOs.Auth;

public class ConfirmEmailDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Token { get; set; } = string.Empty;
}