namespace Ticketing.Infrastructure.Auth;

public class AppUrlSettings
{
    /// <summary>
    /// Public base URL of this API (no trailing slash). Used in emails, e.g. http://localhost:5036 or https://api.example.com
    /// </summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5036";
}
