namespace Ticketing.Infrastructure.Auth;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    /// <summary>Access token lifetime in minutes (binds <c>ExpirationMinutes</c> or <c>ExpiryMinutes</c> from config).</summary>
    public int ExpiryMinutes { get; set; }

    public int ExpirationMinutes
    {
        get => ExpiryMinutes;
        set => ExpiryMinutes = value;
    }

    public int RefreshTokenExpiryDays { get; set; }

    /// <summary>JWT audience claim for refresh tokens (must differ from access-token audience).</summary>
    public string RefreshTokenAudience { get; set; } = "TicketingAPIRefresh";
}