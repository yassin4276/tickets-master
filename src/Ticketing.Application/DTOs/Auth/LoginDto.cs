using System.ComponentModel.DataAnnotations;

namespace Ticketing.Application.DTOs.Auth;

public class LoginDto
{
    [Required]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
