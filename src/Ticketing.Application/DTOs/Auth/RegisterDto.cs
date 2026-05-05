using System.ComponentModel.DataAnnotations;

namespace Ticketing.Application.DTOs.Auth;

public class RegisterDto
{
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = null!;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "User";
}
