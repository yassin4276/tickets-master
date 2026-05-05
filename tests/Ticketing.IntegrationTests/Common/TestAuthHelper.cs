using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Identity;
using Ticketing.Infrastructure.Persistence;

namespace Ticketing.IntegrationTests.Common;

public class TestAuthHelper
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public TestAuthHelper(CustomWebApplicationFactory factory, HttpClient client)
    {
        _factory = factory;
        _client = client;
    }

    public async Task<ApplicationUser> CreateConfirmedEventOwnerAsync(
        string? email = null,
        string password = "test123")
    {
        email ??= $"event-owner-{Guid.NewGuid()}@test.com";

        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Test Event Owner",
            PhoneNumber = "01000000000",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create test event owner: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, "EventOwner");

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to assign EventOwner role: {errors}");
        }

        return user;
    }

    public async Task<string> LoginAndGetTokenAsync(
        string email,
        string password = "test123")
    {
        var request = new
        {
            email,
            password
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Login failed. StatusCode: {response.StatusCode}, Body: {body}");
        }

        return ExtractTokenFromResponse(body);
    }

    public void AttachToken(string token)
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ApplicationUser> CreateConfirmedEventOwnerAndLoginAsync(
        string? email = null,
        string password = "test123")
    {
        var user = await CreateConfirmedEventOwnerAsync(email, password);

        var token = await LoginAndGetTokenAsync(user.Email!, password);

        AttachToken(token);

        return user;
    }

    private static string ExtractTokenFromResponse(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);

        var root = document.RootElement;

        // Case 1:
        // {
        //   "success": true,
        //   "data": {
        //     "token": "..."
        //   }
        // }
        if (root.TryGetProperty("data", out var dataElement))
        {
            if (dataElement.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString()
                    ?? throw new Exception("Token value is null.");
            }

            if (dataElement.TryGetProperty("accessToken", out var accessTokenElement))
            {
                return accessTokenElement.GetString()
                    ?? throw new Exception("AccessToken value is null.");
            }
        }

        // Case 2:
        // {
        //   "token": "..."
        // }
        if (root.TryGetProperty("token", out var directTokenElement))
        {
            return directTokenElement.GetString()
                ?? throw new Exception("Token value is null.");
        }

        // Case 3:
        // {
        //   "accessToken": "..."
        // }
        if (root.TryGetProperty("accessToken", out var directAccessTokenElement))
        {
            return directAccessTokenElement.GetString()
                ?? throw new Exception("AccessToken value is null.");
        }

        throw new Exception($"Token was not found in login response. Body: {responseBody}");
    }

    public async Task<int> GetLatestSessionIdForEventAsync(int eventId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.EventSessions.AsNoTracking()
            .Where(s => s.EventId == eventId)
            .OrderByDescending(s => s.Id)
            .Select(s => s.Id)
            .FirstAsync();
    }

    public static async Task<int> ExtractIdFromResponseAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(body);

        var root = document.RootElement;

        return root
            .GetProperty("data")
            .GetProperty("id")
            .GetInt32();
    }
}