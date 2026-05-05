using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Identity;
using Ticketing.IntegrationTests.Common;

namespace Ticketing.IntegrationTests.Auth;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task CreateConfirmedUserAsync(string email, string password, string role)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Test User",
            PhoneNumber = "01000000000",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create test user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to assign role to test user: {errors}");
        }
    }private async Task CreateNotConfirmedUserAsync(string email, string password, string role)
    {
        using var scope = _factory.Services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = "Test User",
            PhoneNumber = "01000000000",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to create test user: {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new Exception($"Failed to assign role to test user: {errors}");
        }
    }

    [Fact]
    public async Task Register_Should_Return_Success_When_Data_Is_Valid()
    {
        var request = new
        {
            firstName = "Yassin",
            lastName = "Ahmed",
            email = "yassin@test.com",
            phoneNumber = "01000000000",
            password = "test123",
            role = "EventOwner"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    [Fact]
    public async Task Register_should_return__error_when_Email_already_exists()
    {
        var request = new{
            firstName = "Yassin",
            lastName = "Ahmed",
            email = "yassin1@test.com",
            phoneNumber = "01000000000",
            password = "test123",
            role = "EventOwner"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/Auth/register", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/Auth/register", request);

        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_should_return_error_when_Role_is_not_valid()
    {
        var request = new{
            firstName = "Yassin",
            lastName = "Ahmed",
            email = "yassin2@test.com",
            phoneNumber = "01000000000",
            password = "test123",
            role = "InvalidRole"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    [Fact]
    public async Task Login_Should_Return_Success_When_Data_Is_Valid_And_Email_Is_Confirmed()
    {
        var email = $"login-{Guid.NewGuid()}@test.com";
        var password = "test123";

        await CreateConfirmedUserAsync(email, password, "EventOwner");

        var request = new
        {
            email,
            password
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_data_is_invalid()
    {
        var request = new
        {
            email = $"notfound-{Guid.NewGuid()}@test.com",
            password = "test123"
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_Should_Return_BadRequest_When_Email_Is_Not_Confirmed()
    {
        var email = $"notconfirmed-{Guid.NewGuid()}@test.com";
        var password = "test123";

        await CreateNotConfirmedUserAsync(email, password, "EventOwner");

        var request = new
        {
            email,
            password
        };

        var response = await _client.PostAsJsonAsync("/api/Auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}