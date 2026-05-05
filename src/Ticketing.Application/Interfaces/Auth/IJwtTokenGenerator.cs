using Ticketing.Application.DTOs.Auth;

namespace Ticketing.Application;

public interface IJwtTokenGenerator
{
    TokenResultDto GenerateToken(string userId, string fullName, string email, string role);
    TokenResultDto GenerateRefreshToken(string userId,string fullName,string email,string role);
    bool ValidateRefreshToken(string refreshToken, out string userId, out string fullName, out string email, out string role);
}
