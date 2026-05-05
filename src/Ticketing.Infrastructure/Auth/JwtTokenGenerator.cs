using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ticketing.Application;
using Ticketing.Application.DTOs.Auth;
using Ticketing.Infrastructure.Auth;

namespace Ticketing.Infrastructure;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public TokenResultDto GenerateRefreshToken(string userId, string fullName, string email, string role)
    {
        var days = _jwtSettings.RefreshTokenExpiryDays > 0 ? _jwtSettings.RefreshTokenExpiryDays : 7;
        var expiresAt = DateTime.UtcNow.AddDays(days);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.RefreshTokenAudience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new TokenResultDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    public TokenResultDto GenerateToken(string userId, string fullName, string email, string role)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("role", role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new TokenResultDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    public bool ValidateRefreshToken(string refreshToken, out string userId, out string fullName, out string email, out string role)
    {
        userId = string.Empty;
        fullName = string.Empty;
        email = string.Empty;
        role = string.Empty;

        if (string.IsNullOrWhiteSpace(refreshToken))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.RefreshTokenAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
            fullName = principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value ?? string.Empty;
            email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
            role = principal.FindFirst("role")?.Value ?? "User";
            return !string.IsNullOrEmpty(userId);
        }
        catch
        {
            return false;
        }
    }
}
