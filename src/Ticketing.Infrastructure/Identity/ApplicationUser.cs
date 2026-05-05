using Microsoft.AspNetCore.Identity;

namespace Ticketing.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
